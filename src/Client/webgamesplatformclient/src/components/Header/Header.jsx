import { NavLink } from 'react-router-dom';
import "./Header.css"
export default function Header() {
  return (
    <header>
        <NavLink to="/home" className="logo">Game Platform</NavLink>
        <div className="burger">☰</div>
    </header>
  )
}