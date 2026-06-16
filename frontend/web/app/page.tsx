import { Badge } from "@/components/ui/Badge";
import { HomeActions } from "@/components/home/HomeActions";
import { CheckCircle2, FileCheck2, Fingerprint, ShieldCheck } from "lucide-react";

export default function HomePage() {
  return (
    <section className="home-grid">
      <div className="hero-panel">
        <Badge tone="blue">Цифровая проверка</Badge>
        <h1>Проверка подлинности образовательных документов</h1>
        <p>
          Система помогает подтвердить, что документ был выдан образовательной организацией,
          не изменялся после регистрации и не был отозван.
        </p>
        <HomeActions />
      </div>

      <div className="flow-panel" aria-label="Схема работы системы">
        <div className="flow-node">
          <FileCheck2 size={24} />
          <span>Файл</span>
        </div>
        <div className="flow-line" />
        <div className="flow-node">
          <Fingerprint size={24} />
          <span>Цифровой отпечаток</span>
        </div>
        <div className="flow-line" />
        <div className="flow-node">
          <ShieldCheck size={24} />
          <span>Сверка с реестром</span>
        </div>
        <div className="flow-line" />
        <div className="flow-node">
          <CheckCircle2 size={24} />
          <span>Результат</span>
        </div>
      </div>

      <div className="metric-card">
        <span className="metric-label">Проверка</span>
        <strong>быстрый результат</strong>
        <p>Пользователь загружает файл и сразу видит статус подлинности документа.</p>
      </div>
      <div className="metric-card accent">
        <span className="metric-label">Для защиты</span>
        <strong>понятный сценарий</strong>
        <p>В демонстрации видны регистрация, проверка, изменение файла и отзыв документа.</p>
      </div>
    </section>
  );
}
