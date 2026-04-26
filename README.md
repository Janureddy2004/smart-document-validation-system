# Smart Document Validation System
## Demo Video
[Watch Demo](https://drive.google.com/file/d/18RtdKyjPECWXIpSQHr4Tup9YcES8osRH/view?usp=sharing)
## Overview
This project is an AI-based document validation system designed to process insurance PDFs. It extracts key information from documents and validates it against structured data using both rule-based logic and semantic search.

The system combines OCR, Large Language Models (LLMs), and a vector database (Pinecone) to perform intelligent validation and suggest corrections when mismatches are detected.

---

## Features
- Upload PDF documents for processing
- Extract text using OCR
- Convert unstructured text into structured fields using LLM
- Validate extracted data against Excel records
- Use vector database (Pinecone) for semantic search (RAG)
- Suggest corrections for mismatched fields
- Send email notifications based on validation results
- Simple frontend interface built with React

---

## Architecture
PDF → OCR → LLM → Structured Data  
→ Pinecone (Vector Database)  
→ Validation (Rule-based + LLM reasoning)  
→ Email Notification  

---

## Tech Stack
- Backend: .NET Web API  
- Frontend: React (Vite)  
- LLM: Ollama (Llama3, nomic-embed-text)  
- Vector Database: Pinecone  
- OCR: Tesseract  
- Email: SMTP  

---

## Setup Instructions

### Prerequisites
- .NET SDK installed
- Node.js and npm installed
- Ollama installed and running locally
- Pinecone account (API key and index created)

---

### Backend Setup
1. Navigate to backend folder:
      cd SmartDocValidation   

2. Update configuration (appsettings.json):
   - Add Pinecone API Key
   - Add Pinecone Index URL
   - Add Email credentials (if using email feature)

3. Run the backend:
      dotnet run   

---

### Frontend Setup
1. Navigate to frontend folder:
      cd smart-doc-ui   

2. Install dependencies:
      npm install   

3. Run frontend:
      npm run dev   

4. Open browser:
      http://localhost:5173   

---

## Notes
- The system uses a local LLM (Ollama), so it requires local execution.
- For production deployment, Ollama can be replaced with cloud-based LLM APIs such as OpenAI.
- Pinecone is used for vector search to enable semantic validation.

---

## Future Improvements
- Deploy backend using cloud LLM
- Add confidence scoring for validation
- Improve UI/UX
- Support multiple document formats
- Add user authentication

---

## Author
Jahnavi
