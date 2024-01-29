import { Fragment, memo } from 'react';
import Plot from 'react-plotly.js'
//memotization prevents unecessary re-rendering
const RenderDataPoints = memo(function RenderDataPoints({ datapoints, polyfitScalars }) {
    let xData = datapoints.map(item => item.XCoor);
    let yData = datapoints.map(item => item.YCoor);
    // Due to the limitation of plotly, must resort to finding the max and min of x's range to draw the curve
    let xMax = Math.max.apply(null, xData);
    let xMin = Math.min.apply(null, xData);
    function calculateXYFromPolynomial(x, coefficients) {
        return coefficients.reduce((sum, coefficient, index) => sum + coefficient * Math.pow(x, index), 0);
    }
    function generatePolynomialArrays(polyArr, lowBound, upBound, step){
        const xArr = [];
        const yArr = [];
    
        for (let x = lowBound; x <= upBound; x += step) {
            xArr.push(x);
            const y = calculateXYFromPolynomial(x, polyArr);
            yArr.push(y);
        }
        return { xArr, yArr };
    }

    let {xArr, yArr} = generatePolynomialArrays(polyfitScalars, xMin-(xMax-xMin)/8, xMax+(xMax-xMin)/8, (xMax-xMin)/100);

    return(
        <Fragment>
        <Plot
        data={[
            {
              x: xData,
              y: yData,
              type: 'scatter',
              mode: 'markers',
              name: 'Input data points',
            },
            {
                x: xArr,
                y: yArr,
                type: 'scatter',
                mode: 'lines',
                name: 'Curve of best fit',
                line: {
                    color: 'rgb(219, 64, 82)',
                    width: 3
              },
            },
          ]}
          layout={{
                  width: 650,
                  length: 500,
                  plot_bgcolor: 'rgb(0, 0, 0, 0)',
                  paper_bgcolor: 'rgb(0, 0, 0, 0)',
                  margin: {
                      t: 25, //top margin
                      l: 45, //left margin
                      r: 45, //right margin,
                      b: 45 //bottom margin}
                  },
                  yaxis: {
                      autorange: true,
                      showgrid: true,
                      showline: true,
                      mirror: 'ticks',
                      gridwidth: 1,
                      tickfont: {
                          size: 14
                      },
                      title: {
                          text: "Y values"
                      },
                  },
                  xaxis: {
                      tickfont: {
                          size: 14
                      },
                      title: {
                          text: "X values",
                          font: {
                              size: 14
                          }
                      },
                      showline: true,
                      mirror: 'ticks',
                  },
                  legend: {
                    bgcolor: 'rgb(255, 255, 255, 0)',
                    bordercolor: 'rgb(0, 0, 0, 1)',
                    borderwidth: 1,
                  },
              }}
        />
        </Fragment>
        
    );
}
);

export default RenderDataPoints;