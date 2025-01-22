import React from 'react';
import { NavLink, Link } from 'react-router-dom';
import './Header.css'
import 'font-awesome/css/font-awesome.min.css';

export default function Header({ logout, name }) {
  return (
    <header>
      <NavLink to="/home" className="logo">Game Platform</NavLink>
      <Link to={`/profile/${name}`} className="name">{name}</Link>
      <button className="logout-button" onClick={logout}>
        <i className="fa fa-sign-out logout-anim" aria-hidden="true"></i>
      </button>
  </header>
  );
}