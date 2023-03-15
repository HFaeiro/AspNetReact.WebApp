import React, { Component, Profiler, useEffect } from 'react';

import { Navbar, Nav } from 'react-bootstrap';
import { NavLink, Link, useLocation } from 'react-router-dom'
import './Navigation.css';
export default function Navigation(props) {



    //if user is logged in we will show logout instead of login



    //fill navbar with links and dynamic content. 
    return (
       <>
          
            <section className="sideWrapper">
                <div className="sidebar">                
                    <Link className="navLink" to="/">
                        <div> Home</div>
                    </Link>
                    <Link className="navLink" to="/videoapp">
                        <div>Video App</div>
                    </Link>
                    <Link className="navLink" to="/github">
                        <div>Github</div>
                    </Link>
                    <Link className="navLink" to="/youtube">
                        <div>Youtube</div>
                    </Link>
                    <Link className="navLink" to="/downloads">
                        <div>Downloads</div>
                    </Link>
                    <Link className="navLink" to="/friends">
                        <div>My Friends</div>
                    </Link>


                </div>
            
            </section>
        </>
    );
}

