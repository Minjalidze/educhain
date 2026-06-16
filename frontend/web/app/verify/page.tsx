"use client";

import { StateMessage } from "@/components/ui/StateMessage";
import { verifyDocument } from "@/lib/api";
import type { VerificationStatus, VerifyDocumentResult } from "@/types/api";
import { AlertTriangle, CheckCircle2, SearchCheck, ShieldCheck, XCircle } from "lucide-react";
import { FormEvent, useState } from "react";

const resultCopy: Record<VerificationStatus, { title: string; description: string; icon: typeof CheckCircle2 }> = {
  Valid: {
    title: "Документ подлинный",
    description: "Файл совпадает с записью в реестре и не был отозван.",
    icon: CheckCircle2
  },
  NotFound: {
    title: "Не найден",
    description: "В реестре нет записи для этого файла. Возможно, документ не регистрировали.",
    icon: AlertTriangle
  },
  Revoked: {
    title: "Отозван/изменён",
    description: "Документ найден, но его статус больше не считается действительным.",
    icon: XCircle
  }
};

export default function VerifyPage() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [result, setResult] = useState<VerifyDocumentResult | null>(null);

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setLoading(true);
    setError(null);
    setResult(null);

    try {
      const response = await verifyDocument(new FormData(event.currentTarget));
      setResult(response);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Проверка не выполнена.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <section className="form-layout">
      <div className="section-heading">
        <ShieldCheck size={28} />
        <div>
          <h1>Проверка документа</h1>
          <p>Загрузите файл, чтобы сверить его с зарегистрированной записью.</p>
        </div>
      </div>

      <form className="card form-card" onSubmit={onSubmit}>
        <label>
          Файл для проверки
          <input name="File" required type="file" accept=".pdf,.png,.jpg,.jpeg,.txt" />
        </label>
        {error && <StateMessage type="error">{error}</StateMessage>}
        <button className="button button-primary" disabled={loading} type="submit">
          <SearchCheck size={18} />
          {loading ? "Проверка..." : "Проверить документ"}
        </button>
      </form>

      {result && (
        <div className={`result-status status-${result.status.toLowerCase()}`}>
          <div className="status-icon">
            {(() => {
              const Icon = resultCopy[result.status].icon;
              return <Icon size={34} />;
            })()}
          </div>
          <div className="status-content">
            <span>Результат проверки</span>
            <h2>{resultCopy[result.status].title}</h2>
            <p>{resultCopy[result.status].description}</p>
            <dl className="details">
              <dt>Отпечаток файла</dt>
              <dd>{result.documentHash}</dd>
              {result.document && (
                <>
                  <dt>Документ</dt>
                  <dd>{result.document.title}</dd>
                  <dt>Номер</dt>
                  <dd>{result.document.documentNumber}</dd>
                </>
              )}
            </dl>
          </div>
        </div>
      )}
    </section>
  );
}
