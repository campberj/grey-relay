import { Link, NavLink, Outlet, Route, Routes } from 'react-router'
import { CampaignLibrary } from '../features/campaign-library/CampaignLibrary'
import { OperationsCenter } from '../features/operations-center/OperationsCenter'

export function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<CampaignLibrary />} />
        <Route path="operations" element={<OperationsCenter />} />
        <Route path="*" element={<NotFound />} />
      </Route>
    </Routes>
  )
}

function Layout() {
  return (
    <>
      <a className="skip-link" href="#main">
        Skip to content
      </a>
      <header className="app-header">
        <span className="app-header__brand">GREY RELAY</span>
        <nav aria-label="Primary">
          <ul className="app-nav">
            <li>
              <NavLink to="/" end>
                Campaign Library
              </NavLink>
            </li>
            <li>
              <NavLink to="/operations">Operations Center</NavLink>
            </li>
          </ul>
        </nav>
      </header>
      <main id="main" className="app-main" tabIndex={-1}>
        <Outlet />
      </main>
    </>
  )
}

function NotFound() {
  return (
    <section aria-labelledby="not-found-heading">
      <h1 id="not-found-heading">Page not found</h1>
      <p>
        <Link to="/">Return to the Campaign Library</Link>
      </p>
    </section>
  )
}
