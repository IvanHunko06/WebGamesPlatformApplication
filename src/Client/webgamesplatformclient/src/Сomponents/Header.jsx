import React from 'react';
import { NavLink } from 'react-router-dom';

export default function Header() {
  return (
    <header>
        <NavLink to="/home" className="logo">Game Platform</NavLink>
        <div className="burger">☰</div>
    </header>
  )
}
