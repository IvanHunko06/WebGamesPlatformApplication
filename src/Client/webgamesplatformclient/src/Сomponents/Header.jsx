import React from 'react';
import { NavLink } from 'react-router-dom';

export default function Header({ logout }) {
  return (
    <header>
      <NavLink to="/home" className="logo">Game Platform</NavLink>
      <button className="logout-button" onClick={logout}>
        Logout
      </button>
      <div className="burger">☰</div>
  </header>
  );
}
