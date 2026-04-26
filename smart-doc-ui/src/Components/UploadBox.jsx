import { useState } from "react";

function UploadBox({ onUpload, loading }) {
  const [file, setFile] = useState(null);

  const handleClick = () => {
    if (!file) {
      alert("Please select a file");
      return;
    }
    onUpload(file);
  };

  return (
    <div className="upload-box">
      <input
        type="file"
        onChange={(e) => setFile(e.target.files[0])}
      />

      <button onClick={handleClick}>
        {loading ? "Processing..." : "Upload & Validate"}
      </button>
    </div>
  );
}

export default UploadBox;