"use client";

import { RequireAuth } from "@/components/auth/RequireAuth";
import { DocumentStatusBadge } from "@/components/ui/Badge";
import { StateMessage } from "@/components/ui/StateMessage";
import { listDocuments, revokeDocument } from "@/lib/api";
import type { DocumentDto } from "@/types/api";
import { FileCheck2, RotateCcw } from "lucide-react";
import { useEffect, useState } from "react";

export default function DocumentsPage() {
  return (
    <RequireAuth roles={["Admin", "Issuer"]}>
      <DocumentsContent />
    </RequireAuth>
  );
}

function DocumentsContent() {
  const [documents, setDocuments] = useState<DocumentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  async function load() {
    setLoading(true);
    setError(null);

    try {
      setDocuments(await listDocuments());
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Список документов недоступен.");
    } finally {
      setLoading(false);
    }
  }

  async function revoke(id: string) {
    setError(null);

    try {
      await revokeDocument(id);
      await load();
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Не удалось отозвать документ.");
    }
  }

  useEffect(() => {
    load();
  }, []);

  return (
    <section className="stack">
      <div className="section-heading">
        <FileCheck2 size={28} />
        <div>
          <h1>Документы</h1>
          <p>Реестр документов, зарегистрированных в системе.</p>
        </div>
      </div>

      {error && <StateMessage type="error">{error}</StateMessage>}
      {loading && <StateMessage type="info">Загрузка списка...</StateMessage>}

      <div className="card table-card">
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Название</th>
                <th>Номер</th>
                <th>Тип</th>
                <th>Дата</th>
                <th>Отпечаток</th>
                <th>Статус</th>
                <th>Запись</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {documents.map((document) => (
                <tr key={document.id}>
                  <td>{document.title}</td>
                  <td>{document.documentNumber}</td>
                  <td>{document.documentType}</td>
                  <td>{document.issueDate}</td>
                  <td className="mono">{document.documentHash.slice(0, 18)}...</td>
                  <td>
                    <DocumentStatusBadge revoked={document.isRevoked} />
                  </td>
                  <td className="mono">{document.blockchainTransactionHash.slice(0, 18)}...</td>
                  <td>
                    <button
                      className="icon-button danger"
                      disabled={document.isRevoked}
                      onClick={() => revoke(document.id)}
                      title="Отозвать"
                      type="button"
                    >
                      <RotateCcw size={17} />
                    </button>
                  </td>
                </tr>
              ))}
              {!loading && documents.length === 0 && (
                <tr>
                  <td colSpan={8}>Документы пока не добавлены.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </section>
  );
}
