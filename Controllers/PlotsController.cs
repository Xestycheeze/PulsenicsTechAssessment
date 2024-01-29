using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using MathNet.Numerics;
using Microsoft.Win32;
using PulsenicsTechAs.Interfaces;
using PulsenicsTechAs.Models;
using PulsenicsTechAs.Utils;

namespace PulsenicsTechAs.Controllers
{
        public class PlotsController : ApiController
    {
        [Route("api/AddNewPlot")]
        [HttpPost]
        public ResponsePostModel AddPlot(RequestPostModel plot)
        {
            int oldPlotId = plot.Id;
            int polyPower = plot.PolyPower;
            int datapoints = plot.LstAddDatapoint.Count;
            ResponsePostModel responseModel = new ResponsePostModel();
            // Reject most cases of bad inputs, except for curve fitting readiness.
            if (oldPlotId != -1 || plot.LstDeleteDatapoint.Count != 0 || plot.GrossNumbDatapoint != 0 || datapoints <= 0 || polyPower < 0)
            {
                responseModel.ResponseCode = "422";
                responseModel.ResponseMessage = "Mismatched request payload and HTTP methods";
                return responseModel;
            }
            
            if (!UtilsMisc.IsCurveFittingReady(datapoints, polyPower))
            {
                polyPower = 0;
                responseModel.PolyfitPower = polyPower;
                responseModel.ResponseCode = "207";
                responseModel.ResponseMessage = "Plot created successfuly. Insufficient datapoints to compute requested polyfit";
                responseModel.Scalars = new List<double>();
            }
            int newPlotId = DbController.AddPlot(polyPower);
            DbController.AddDatapoints(newPlotId, plot.LstAddDatapoint);
            responseModel.PlotId = newPlotId;
            if (polyPower == 0)
            {
                // Ugly, maybe theres a way to combine the two if statements
                return responseModel;
            }
            double[] xArr = plot.LstAddDatapoint.Select(x => x.XCoor).ToArray();
            double[] yArr = plot.LstAddDatapoint.Select(y => y.YCoor).ToArray();
            double[] curveOfBestFit = Fit.Polynomial(xArr, yArr, order: polyPower);
            List<double> curveofBestFitList = curveOfBestFit.ToList();
            DbController.AddPolyfit(newPlotId, curveofBestFitList);

            responseModel.Scalars = curveofBestFitList;
            responseModel.PolyfitPower = polyPower;
            responseModel.ResponseCode = "200";
            responseModel.ResponseMessage = "Plot created successfuly. New polyfit curve calculated";
            return responseModel;
        }

        [Route("api/DeleteNewPlot")]
        [HttpDelete]
        public void DeletePlot(RequestPostModel plot)
        {
            // The directions never specify the need to delete plots.
        }

        [Route("api/UpdatePlot")]
        [HttpPost] // maybe HttpPut?
        public ResponsePostModel UpdatePlot(RequestPostModel plot)
        {
            ResponsePostModel responseModel = new ResponsePostModel();
            int plotId = plot.Id;
            int polyPower = plot.PolyPower;

            // Check enough datapoints to work with
            if (!UtilsMisc.IsCurveFittingReady(plot.GrossNumbDatapoint + plot.LstAddDatapoint.Count - plot.LstDeleteDatapoint.Count, polyPower))
            {
                responseModel.ResponseCode = "422";
                responseModel.ResponseMessage = "Not enough data points to perform the requested polyfit";
                return responseModel;
            }

            // If the user just wants a different curve of the same dataset just get it from DB
            if (plot.LstDeleteDatapoint.Count == 0 && plot.LstAddDatapoint.Count == 0)
            {
                List<double> doubles = DbController.GetPlotPolyfit(plotId, polyPower);
                if (doubles.Count() > 0)
                {
                    DbController.UpdatePlotPolyfitDisplay(plotId, polyPower);
                    responseModel.Scalars = doubles;
                    responseModel.PolyfitPower = polyPower;
                    responseModel.PlotId = plotId;
                    responseModel.ResponseCode = "200";
                    responseModel.ResponseMessage = "New polyfit curve fetched";
                    return responseModel;
                }
                // if the input is IsCurveFittingReady == true but does not have an archived polyfit in DB,
                // its polyfit will be calculated down the stream

            }
            // Remove and add datapoints to DB. Note That deletion is O(log(n)) time cuz deletion by a primary key
            DbController.DeleteDatapoints(plot.LstDeleteDatapoint);
            DbController.AddDatapoints(plotId, plot.LstAddDatapoint);

            // Calculate the curve of best fit
            List<Tuple<int, double, double>> datapoints = DbController.GetPlotDatapoints(plotId);
            double[] xArr = datapoints.Select(x => x.Item2).ToArray();
            double[] yArr = datapoints.Select(y => y.Item3).ToArray();
            double[] curveOfBestFit = Fit.Polynomial(xArr, yArr, order: polyPower);
            List<double> curveofBestFitList = curveOfBestFit.ToList();

            // Delete all other curves of best fit of the same plot in DB only when the data points have changed
            if (plot.LstDeleteDatapoint.Count > 0 || plot.LstAddDatapoint.Count > 0)
            {
                // make a list of all powers except the one that was just calculated
                List<int> powersToBeDeleted = Enumerable.Range(1, 3).ToList();
                DbController.DeletePolyfit(plotId, powersToBeDeleted);
            }

            // Add newly calculated curve to DB
            DbController.AddPolyfit(plotId, curveofBestFitList);
            DbController.UpdatePlotPolyfitDisplay(plotId, polyPower);
            // Assemble the response
            responseModel.Scalars = curveofBestFitList;
            responseModel.PolyfitPower = polyPower;
            responseModel.PlotId = plotId;
            responseModel.ResponseCode = "200";
            responseModel.ResponseMessage = "New polyfit curve calculated";
            return responseModel;
        }

        [Route("api/GetAllPlots")]
        [HttpGet]
        public ResponseGetModel GetAllPlots()
        {
            ResponseGetModel responseGetModel = new ResponseGetModel();
            List<ResponseGet> lstPlot = DbController.GetAllPlotsBasicInfo();
            if (lstPlot.Count == 0) 
            {
                responseGetModel.ResponseCode = "204";
                responseGetModel.ResponseMessage = "No plots found.";
                responseGetModel.LstPlot = lstPlot;
                return responseGetModel;
            }

            // All datapoints must be associated to some plot, so it is faster to FIRST get everything out of the datapoints table THEN catagorize them
            // Resembles redix sort
            Dictionary<int, List<Tuple<int, double, double>>> allDatapoints = DbController.GetPlotDatapoints();

            foreach (ResponseGet plot in lstPlot)
            {
                int power = plot.PolyPower;
                int plotId = plot.Id;

                // populate the datapoints for a specific graph to said graph
                List<Tuple<int, double, double>> datapoints = allDatapoints[plotId];
                foreach (Tuple<int, double, double> datapoint in datapoints)
                {
                    Datapoint datapointClassProto = new Datapoint { DatapointId = datapoint.Item1, XCoor = datapoint.Item2, YCoor = datapoint.Item3 };
                    plot.LstDatapoint.Add(datapointClassProto);
                }

                // populate the PolyfitPower and Scalars field 
                switch (power)
                {
                    case 0:
                        break;
                    case 1:
                    case 2:
                    case 3:
                        List<double> scalars = DbController.GetPlotPolyfit(plotId, power);
                        if (scalars.Count() == 0)
                        {
                            // If the polyfit DB doesn't have the polynomial available calculate it
                            double[] xArr = datapoints.Select(x => x.Item2).ToArray();
                            double[] yArr = datapoints.Select(y => y.Item3).ToArray();
                            double[] curveOfBestFit = Fit.Polynomial(xArr, yArr, order: power);
                            scalars = curveOfBestFit.ToList();
                            DbController.AddPolyfit(plotId, scalars);
                        }
                        foreach (double scalar in scalars)
                        {
                            plot.Scalars.Add(scalar);
                        }
                        plot.PolyfitPower = power;
                        break;
                    default:
                        throw new Exception("Selected polyfit curve type does not exist.");

                }
            }

            responseGetModel.ResponseCode = "200";
            responseGetModel.ResponseMessage = "All plots fetched";
            responseGetModel.LstPlot = lstPlot;
            return responseGetModel;
        }
    }
}
