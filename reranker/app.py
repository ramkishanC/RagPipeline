from fastapi import FastAPI
from pydantic import BaseModel
from sentence_transformers import CrossEncoder
import torch

app = FastAPI()

model = CrossEncoder("cross-encoder/ms-marco-MiniLM-L6-v2", activation_fn=torch.nn.Sigmoid())


class DocumentItem(BaseModel):
    id: str
    text: str


class RerankRequest(BaseModel):
    query: str
    documents: list[DocumentItem]


@app.get("/health")
def health():
    return {"status": "ok"}


@app.post("/rerank")
def rerank(request: RerankRequest):
    if not request.documents:
        return {"results": []}

    pairs = [(request.query, document.text) for document in request.documents]
    scores = model.predict(pairs)
    results = []

    for document, score in zip(request.documents, scores):
        results.append({"id": document.id, "score": float(score)})

    results.sort(key=lambda x: x["score"], reverse=True)

    return {"results": results}
