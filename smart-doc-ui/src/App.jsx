import { useState } from "react";
import axios from "axios";
import "./App.css";
import UploadBox from "./components/UploadBox";
import ResultCard from "./components/ResultCard";

function App() {
  const [result, setResult] = useState(null);
  const [loading, setLoading] = useState(false);

  const handleUpload = async (file) => {
    if (!file) return alert("Please select a file");

    const formData = new FormData();
    formData.append("file", file);

    try {
      setLoading(true);

      const res = await axios.post(
        "http://localhost:5119/api/Upload/process",
        formData
      );

      setResult(res.data);
    } catch (err) {
      console.error(err);
      alert("Error uploading file");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container">
      <h1>Smart Document Validation</h1>

      {/* 🔥 Upload Component */}
      <UploadBox onUpload={handleUpload} loading={loading} />

      {/* 🔥 Result Component */}
      {result && <ResultCard data={result} />}
    </div>
  );
}

export default App;