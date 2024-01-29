import {Routes, Route} from "react-router-dom";
import Plots from "./pages/Plots";
//import './App.css';

function App() {

  return (
    <div className="App">
      <Routes>
        <Route path="" element={<Plots/>}/>
      </Routes>
    </div>
  );
}

export default App
