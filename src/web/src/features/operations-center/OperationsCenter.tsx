import { Link } from 'react-router'

/** Placeholder until S2 binds the TOC to a saved campaign. Shows no campaign data. */
export function OperationsCenter() {
  return (
    <section aria-labelledby="operations-center-heading">
      <h1 id="operations-center-heading">Operations Center</h1>
      <p className="notice">
        No campaign is open. Choose one in the <Link to="/">Campaign Library</Link>.
      </p>
    </section>
  )
}
