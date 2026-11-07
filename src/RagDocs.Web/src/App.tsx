import { NavLink, Route, Routes } from 'react-router'
import { DocumentsPage } from './documents/DocumentsPage'

export default function App() {
  return (
    <div className="layout">
      <header className="topbar">
        <span className="brand">RagDocs</span>
        <nav>
          <NavLink to="/" end>
            Documents
          </NavLink>
        </nav>
      </header>

      <main>
        <Routes>
          <Route path="/" element={<DocumentsPage />} />
        </Routes>
      </main>
    </div>
  )
}