function ResultCard({ data }) {
  const result = data?.resolutionResult;
  const extracted = data?.structuredData;

  const status = result?.status;
  const decision = result?.decision;

  const suggestions = decision?.suggested_corrections;
  const getStatusClass = (status) => {
  if (status === "SUCCESS") return "badge-success";
  return "badge-fail"; // everything else = red
};
  return (
    <div className="result-card">
      

      {/* ✅ Status */}
<h2>Validation Result</h2>

<span className={getStatusClass(status)}>
  {status}
</span>

      {/* 📄 Extracted Data */}
      <h3>Extracted Data</h3>
      <ul>
        <li><b>Name:</b> {extracted?.name}</li>
        <li><b>Email:</b> {extracted?.email}</li>
        <li><b>Company:</b> {extracted?.company_name}</li>
        <li><b>Policy:</b> {extracted?.policy_number}</li>
      </ul>

      {/* ❌ Suggestions (only if mismatch) */}
      {status !== "SUCCESS" && suggestions && (
        <>
          <h3>Suggested Corrections</h3>

          <div className="corrections">
            {suggestions.name && (
              <p>
                <b>Name:</b> {extracted?.name} → {suggestions.name}
              </p>
            )}

            {suggestions.email && (
              <p>
                <b>Email:</b> {extracted?.email} → {suggestions.email}
              </p>
            )}

            {suggestions.company_name && (
              <p>
                <b>Company:</b> {extracted?.company_name} → {suggestions.company_name}
              </p>
            )}
          </div>
        </>
      )}

      {/* 🧠 Reason */}
      {decision?.reason && (
        <>
          <h3>Reason</h3>
          <p>{decision.reason}</p>
        </>
      )}
    </div>
  );
}

export default ResultCard;