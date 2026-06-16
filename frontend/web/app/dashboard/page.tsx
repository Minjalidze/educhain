"use client";

import { RequireAuth } from "@/components/auth/RequireAuth";
import { getDashboard } from "@/lib/api";
import type { DashboardStats } from "@/types/api";
import { Activity, FileCheck2, PlusCircle, RotateCcw, SearchCheck } from "lucide-react";
import Link from "next/link";
import { useEffect, useState } from "react";
import { DocumentStatusBadge } from "@/components/ui/Badge";
import { StateMessage } from "@/components/ui/StateMessage";

export default function DashboardPage() {
  return (
    <RequireAuth roles={["Admin", "Issuer"]}>
      <DashboardContent />
    </RequireAuth>
  );
}

function DashboardContent() {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getDashboard()
      .then(setStats)
      .catch((exception) => setError(exception instanceof Error ? exception.message : "Сводка недоступна."));
  }, []);

  return (
    <section className="stack">
      <div className="section-heading">
        <Activity size={28} />
        <div>
          <h1>Сводка</h1>
          <p>Операционная сводка по зарегистрированным документам.</p>
        </div>
      </div>

      {error && <StateMessage type="error">{error}</StateMessage>}
      {!stats && !error && <StateMessage type="info">Загрузка данных...</StateMessage>}

      {stats && (
        <>
          <div className="stats-grid">
            <div className="card stat">
              <FileCheck2 size={22} />
              <span>Всего документов</span>
              <strong>{stats.totalDocuments}</strong>
            </div>
            <div className="card stat">
              <SearchCheck size={22} />
              <span>Активные</span>
              <strong>{stats.activeDocuments}</strong>
            </div>
            <div className="card stat">
              <RotateCcw size={22} />
              <span>Отозванные</span>
              <strong>{stats.revokedDocuments}</strong>
            </div>
            <div className="card stat">
              <Activity size={22} />
              <span>Проверки</span>
              <strong>{stats.verificationChecks}</strong>
            </div>
          </div>

          <div className="actions-row">
            <Link className="button button-primary" href="/documents/new">
              <PlusCircle size={18} />
              Добавить документ
            </Link>
            <Link className="button button-secondary" href="/documents">
              <FileCheck2 size={18} />
              Список документов
            </Link>
          </div>

          <div className="card table-card">
            <h2>Последние документы</h2>
            <div className="table-wrap">
              <table>
                <thead>
                  <tr>
                    <th>Название</th>
                    <th>Номер</th>
                    <th>Дата</th>
                    <th>Статус</th>
                  </tr>
                </thead>
                <tbody>
                  {stats.recentDocuments.map((document) => (
                    <tr key={document.id}>
                      <td>{document.title}</td>
                      <td>{document.documentNumber}</td>
                      <td>{document.issueDate}</td>
                      <td>
                        <DocumentStatusBadge revoked={document.isRevoked} />
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </>
      )}
    </section>
  );
}
