import { useState, useEffect } from "react";
import axios from "axios";
import RenderDataPoints from "../components/RenderDataPoints";

class BackendData {
    constructor(Id, LstDatapoint, PlotId, PolyPower, PolyfitPower, Scalars) {
        this.Id = Id;
        this.LstDatapoint = LstDatapoint || [];
        this.PlotId = PlotId;
        this.PolyPower = PolyPower; // The power of curve the user selects. May desync with PolyfitPower by design
        this.PolyfitPower = PolyfitPower; // The power of the curve that is currently on display in a certain plot
        this.Scalars = Scalars || [];
    }
}

class BackendDataArr extends Array {
    constructor(...items){
        super(...items.map(item => new BackendData(item.Id , item.LstDatapoint , item.PlotId , item.PolyPower , item.PolyfitPower , item.Scalars)))
    }
}
function Plots(){

    var API_URL = "https://localhost:44350/"; // The API backend may be configured to other URLS. Modify accordingly.

    const [httpResponseCode, setHttpResponseCode] = useState("N/A");
    const [httpResponseMsg, setHttpResponseMsg] = useState("N/A");
    const [backendDataArr, setBackendDataArr] = useState([]);

    // Assignment never specified the need to handle data point deletion
    //const [delNewXYPairArr, setDelNewXYPairArr] = useState([]);
    const [addNewXYPairArr, setAddNewXYPairArr] = useState([]);
    const [latestX, setLatestX] = useState('');
    const [latestY, setLatestY] = useState('');

    const [selectedNewPlotPolyPower, setSelectedNewPlotPolyPower] = useState(0);

    const [focusedPlotId, setFocusedPlotId] = useState(-1);
    const [focusedPolyPower, setFocusedPolyPower] = useState(0);
    const [focusedGrossNumbDatapoint, setFocusedGrossNumbDatapoint] = useState(-1);

    ///////////var bd = new BackendData();
    useEffect(() => {
        const getBackendData = async () => {
            try {
                const response = await axios.get(`${API_URL}/api/GetAllPlots`);
                setHttpResponseCode(response.data['ResponseCode'])
                setHttpResponseMsg(response.data['ResponseMessage'])
                console.log(response);
                const backendData = response.data.LstPlot;
                const dataArr = new BackendDataArr(...backendData);
                setBackendDataArr(dataArr);
            } catch (err){
                console.error('Error fetching data:', err)
            }
        };

        getBackendData();
        
    // eslint-disable-next-line react-hooks/exhaustive-deps
    },[]);

    function polyPowerName(power) {
        let name;       
        switch (power){
            case 1:
                name = "Linear";
                break;
            case 2:
                name = "Quadratic";
                break;
            case 3:
                name = "Cubic";
                break;
            default:
                name = "This curve fitting degree is not available";
        }
        return name
    }

    function handleHover(focusId, focusPower, focusGross) {
        setFocusedPlotId(focusId);
        setFocusedPolyPower(focusPower);
        setFocusedGrossNumbDatapoint(focusGross);
    }

    const isPolyfitPossible = (degree) => {
        return (focusedGrossNumbDatapoint + addNewXYPairArr.length) > degree
    };

    const isNumeric = (num) => (typeof(num) === 'number' || typeof(num) === "string" && num.trim() !== '') && !isNaN(num);

    function HandleAddDatapoint() {
        if (isNumeric(latestX.trim()) && isNumeric(latestY.trim())) {
            let combinedXY = {"DatapointId": -1, "XCoor": Number(latestX.trim()), "YCoor": Number(latestY.trim())}
            addNewXYPairArr.length ? setAddNewXYPairArr([...addNewXYPairArr, combinedXY]) : setAddNewXYPairArr([combinedXY]);
            setLatestX('');
            setLatestY('');
        }
    }
    
    function HandleDeleteDatapoint() {
        // The directions never specify the need to delete data points.
    }

    async function HandleRadioButtonChange(e) {
        // only allow when number of new data points plus current data points in plot is greatere than selected power
        if (isPolyfitPossible(Number(e.target.value))) {
            if (focusedPlotId === -1) {
                setSelectedNewPlotPolyPower(Number(e.target.value));
                setFocusedPolyPower(Number(e.target.value))
                return;
            } 
            UpdateProperty(focusedPlotId, 'PolyPower', Number(e.target.value));
            if (focusedGrossNumbDatapoint <= Number(e.target.value)) {
                // if the data points already in current plot could not compute for the selected curve,
                // only update PolyPower, retaining the radio button selection but not (yet) compute
                return;
            }
            // Regardless if there are new data points or not, attempt to get the curve from backend with the old data points
            // The user can see the curve with new data points once they confirm adding the data points into the plot
            const formData = {
                Id : focusedPlotId,
                PolyPower: Number(e.target.value),
                GrossNumbDatapoint : focusedGrossNumbDatapoint,
                LstAddDatapoint : [],
                LstDeleteDatapoint : []
            }

            const response = await axios.post(`${API_URL}/api/UpdatePlot`, formData);
            setHttpResponseCode(response.data['ResponseCode']);
            setHttpResponseMsg(response.data['ResponseMessage']);
            const backendData = response.data;
            if (backendData['ResponseCode'] === "200") {
                UpdateProperty(focusedPlotId, 'PolyfitPower', Number(e.target.value));
                UpdateProperty(focusedPlotId, 'Scalars', backendData.Scalars);
            }
        }
    }

    function UpdateProperty(elementId, propertyName, newValue){
        setBackendDataArr(prevArray => {
            // Find the element by ID and update the specified property
            const newArray = prevArray.map(item => {
                if (item.Id === elementId) {
                    return { ...item, [propertyName]: newValue };
                }
                return item;
            });

            return newArray;
        });
    }
    
    async function HandleMakeNewPlot() {
        if (isPolyfitPossible(selectedNewPlotPolyPower) && selectedNewPlotPolyPower >= 1) {
            try {
            const formData = {
                Id : focusedPlotId,
                PolyPower: focusedPolyPower,
                GrossNumbDatapoint : focusedGrossNumbDatapoint,
                LstAddDatapoint : addNewXYPairArr,
                LstDeleteDatapoint : []
            }

            const response = await axios.post(`${API_URL}/api/AddNewPlot`, formData);
            setHttpResponseCode(response.data['ResponseCode'])
            setHttpResponseMsg(response.data['ResponseMessage'])
            const backendData = response.data;
            if (backendData['ResponseCode'] === "200") {
                const newPlot = new BackendData(
                    backendData.PlotId,
                    addNewXYPairArr, 
                    backendData.PlotId, 
                    backendData.PolyfitPower, 
                    backendData.PolyfitPower, 
                    backendData.Scalars);
                setBackendDataArr([...backendDataArr, newPlot]);
                setAddNewXYPairArr([]); // Reset the new datapoint array if successfully add new plot
            }
            
            } catch (err) {
                console.error('Error fetching data:', err);
            }
        }
    }

    async function HandleUpdateExistingPlot() {
        try {
            const formData = {
                Id : focusedPlotId,
                PolyPower: focusedPolyPower,
                GrossNumbDatapoint : focusedGrossNumbDatapoint,
                LstAddDatapoint : addNewXYPairArr,
                LstDeleteDatapoint : []
            }

            const response = await axios.post(`${API_URL}/api/UpdatePlot`, formData);
            setHttpResponseCode(response.data['ResponseCode']);
            setHttpResponseMsg(response.data['ResponseMessage']);
            const backendData = response.data;
            if (backendData['ResponseCode'] === "200") {
                // the response will give the id of the modified plot. use that to update that spefic plot already stored in the frontend
                UpdateProperty(backendData.PlotId, 'LstDatapoint', backendDataArr.find(item => item.Id === backendData.PlotId).LstDatapoint.concat(addNewXYPairArr));
                UpdateProperty(backendData.PlotId, 'PolyPower', backendData.PolyfitPower);
                UpdateProperty(backendData.PlotId, 'PolyfitPower', backendData.PolyfitPower);
                UpdateProperty(backendData.PlotId, 'Scalars', backendData.Scalars);
                setAddNewXYPairArr([]);
            }
            
        } catch (err) {
            console.error('Error fetching data:', err);
        }

        
    }
    
    function HandleDeletePlot() {
        // The directions never specify the need to delete plots.
    }
    return(
        <div>{/*
            {backendDataArr.length ? (<div>
                <table><thead>
                <tr>
                    <th>X Value</th>
                    <th>Y Value</th>
                </tr>
                </thead><tbody>{backendDataArr.map((dict, index) => (
                        <tr key={index}>
                        <td>{dict.Id}</td>
                        <td>{dict.LstDatapoint.toString()}</td>
                        </tr>
                ))}</tbody></table></div>
                ) : (
                    <div></div>
                )}
            <div>focusedGrossNumbDatapoint {focusedGrossNumbDatapoint} </div>
            <div>addNewXYPairArr.length {addNewXYPairArr.length}</div>
            <div> focusedPolyPower{focusedPolyPower}</div>
            <div>selectedNewPlotPolyPower{selectedNewPlotPolyPower}</div>*/}
            <div>Backend Status: {httpResponseCode}</div>
            <div>Backend Response Message: {httpResponseMsg}</div>
                
            <div>
                <div onMouseEnter={() => handleHover(-1, selectedNewPlotPolyPower, 0)}
                    onMouseLeave={() => handleHover(-1, 0, -1)}
                    style={{ border: '1px solid black', padding: '10px', margin: '5px' }}>
                    Choose a curve of best fit:
                    {[1,2,3].map((option => (
                                <label key={option}>
                                    <input
                                    type="radio"
                                    value={`${option}`}
                                    checked={selectedNewPlotPolyPower === option}
                                    onChange={(e) => HandleRadioButtonChange(e)}
                                    disabled={!isPolyfitPossible(option)}
                                    />
                                    <span>{polyPowerName(option)}</span>

                                </label>
                            )))}
                            &nbsp;
                    <button onClick={HandleMakeNewPlot}
                        disabled={!(selectedNewPlotPolyPower > 0)}
                    >Add data points to a new plot</button>
                </div>
                
                
                <input 
                    type="number" 
                    inputMode="numeric" 
                    onChange={e => setLatestX(e.target.value)} 
                    value={latestX}
                    placeholder="Enter new X value"
                />
                <input 
                    type="number" 
                    inputMode="numeric" 
                    onChange={e => setLatestY(e.target.value)} 
                    value={latestY}
                    placeholder="Enter new Y value"
                />
                <button onClick={HandleAddDatapoint}>Add data point </button>

                <table style={{ border: '1px solid black', padding: '10px', margin: '5px' }}>
                <thead>
                <tr>
                    <th>X Value</th>
                    <th>Y Value</th>
                </tr>
                </thead>
                <tbody>
                {addNewXYPairArr.length ? (
                    addNewXYPairArr.map((dict, index) => (
                        <tr key={index}>
                        <td>{dict.XCoor}</td>
                        <td>{dict.YCoor}</td>
                        </tr>
                    ))
                    ) : (
                    <tr key={0}>
                    <td>N/A</td>
                    <td>N/A</td>
                    </tr>
                    )
                }
                </tbody>
            </table>
            </div>
            <div>
                {backendDataArr.length ? (
                    backendDataArr.map((item, index) => (
                    <div id={index} 
                    key={index}
                    style={{ border: '1px solid black', padding: '10px', margin: '5px' }}
                    onMouseEnter={() => handleHover(item.PlotId, item.PolyPower, item.LstDatapoint.length)}
                    onMouseLeave={() => handleHover(-1, 0, -1)} // Reset values when leaving the div
                    >
                        <p>Plot id: {item.PlotId}</p>
                        <p>Number of datapoints in this plot: {item.LstDatapoint.length}</p>
                        <p>Displaying Polyfit: {item.PolyPower < 1 ? "N/A" :item.PolyPower}</p>
                        { // only display polynomial only when the Scalar array is not empty 
                        !!(item.Scalars.length) &&
                        <p><em>y =&nbsp;
                        {item.Scalars.map((item, index) => (
                            <span key={index}>{index === 0 ? '' : ' + '}a{index}{index === 0 ? '' : `*x^${index}`}</span>
                        )) }
                        </em>, where</p>
                        }
                        <p>{item.Scalars.reduce((s,x,i) => s+(i>=0 ? ` a${i} = `: '' ) + (x == null ? '' : `${x},`), ' ')}</p>
                        
                        <RenderDataPoints datapoints={item.LstDatapoint} polyfitScalars={item.Scalars}/>
                        <div>{
                            [1,2,3].map((option => (
                                <label key={option}>
                                    <input
                                    type="radio"
                                    value={`${option}`}
                                    checked={item.PolyPower === option}
                                    onChange={(e) => HandleRadioButtonChange(e)}
                                    disabled={!isPolyfitPossible(option)}
                                    />
                                    <span>{polyPowerName(option)}</span>
                                    
                                </label>
                            )))
                        }</div>
                        <div>
                            <div>Note: Selecting the curve options will only apply to the datapoints already in the plot.</div>
                            <div>Cick on the button below to apply new data points to the plot.</div>
                            <button onClick={HandleUpdateExistingPlot}> 
                                Add data points to this plot
                            </button>
                        </div>
                    

                    </div>
                    ))
                ) : (
                    <div></div>
                )}
                {/**/}
            </div>
            
        </div>
    );
}
export default Plots;