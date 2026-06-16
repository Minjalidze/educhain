"use client";

import { RequireAuth } from "@/components/auth/RequireAuth";
import { StateMessage } from "@/components/ui/StateMessage";
import { addDocument } from "@/lib/api";
import type { AddDocumentResult } from "@/types/api";
import { FileUp, PlusCircle } from "lucide-react";
import { FormEvent, useState } from "react";

export default function NewDocumentPage() {
  return (
    <RequireAuth roles={["Admin", "Issuer"]}>
      <NewDocumentContent />
    </RequireAuth>
  );
}

function NewDocumentContent() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [result, setResult] = useState<AddDocumentResult | null>(null);

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setLoading(true);
    setError(null);
    setResult(null);

    const form = event.currentTarget;
    const formData = new FormData(form);

    try {
      const response = await addDocument(formData);
      setResult(response);
      form.reset();
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Документ не добавлен.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <section className="form-layout wide">
      <div className="section-heading">
        <FileUp size={28} />
        <div>
          <h1>Добавление документа</h1>
          <p>Заполните сведения о документе и прикрепите файл для регистрации.</p>
        </div>
      </div>

      <form className="card form-card" onSubmit={onSubmit}>
        <div className="form-grid">
          <label>
            Название
            <input name="Title" required placeholder="Диплом СПО" />
          </label>
          <label>
            Номер
            <input name="DocumentNumber" required placeholder="NKEIVT-2026-001" />
          </label>
          <label>
            Тип
            <input name="DocumentType" required placeholder="Диплом" />
          </label>
          <label>
            Дата выдачи
            <input name="IssueDate" required type="date" />
          </label>
        </div>
        <label>
          Файл
          <input name="File" required type="file" accept=".pdf,.png,.jpg,.jpeg,.txt" />
        </label>

        {error && <StateMessage type="error">{error}</StateMessage>}

        <button className="button button-primary" disabled={loading} type="submit">
          <PlusCircle size={18} />
          {loading ? "Добавление..." : "Добавить в реестр"}
        </button>
      </form>

      {result && (
        <div className="card result-card">
          <h2>Документ зарегистрирован</h2>
          <dl className="details">
            <dt>Отпечаток файла</dt>
            <dd>{result.documentHash}</dd>
            <dt>Запись в реестре</dt>
            <dd>{result.transactionHash}</dd>
          </dl>
        </div>
      )}
    </section>
  );
}
