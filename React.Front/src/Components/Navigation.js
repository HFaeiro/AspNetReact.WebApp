import React, { Component, Profiler, useEffect } from 'react';

import { Navbar, Nav } from 'react-bootstrap';
import { NavLink, Link, useLocation } from 'react-router-dom'
import './Navigation.css';
export default function Navigation(props) {



    //if user is logged in we will show logout instead of login
    let NavLog = props.isLoggedIn === 'false' ? (
        <>
            <NavLink className="d-inline p-2 bg-dark text-white" to="/login">
                Login
            </NavLink>
            <NavLink className="d-inline p-2 bg-dark text-white" to="/create">
                Create an Account
            </NavLink>
        </>
    ) : (

        <>
                 <NavLink className="d-inline p-2 bg-dark text-white" to="/logout">
                Logout
                </NavLink>
                <NavLink className="d-inline p-2 bg-dark text-white" to="/myvideos">
                    My Videos
                </NavLink>
                {/*props.profile.privileges == 'Admin' ?*/ /*// if user is admin we will show Users link.*/
                    <> <NavLink className="d-inline p-2 bg-dark text-white" to="/users">
                    Users
                </NavLink>

        </>
                 
                     }
        </>


        )



    //fill navbar with links and dynamic content. 
    return (
        <Navbar className="mainNavbar" bg="dark" expand="lg">
            <Navbar.Toggle aria-controls="basic-navbar-nav"></Navbar.Toggle>
            <Navbar.Collapse id="basic-navbar-nav">
                <Nav className="ml-auto">
                    <NavLink className="d-inline p-2 bg-dark text-white" to="/">
                        Home
                    </NavLink>
                    {NavLog}


                </Nav>
            </Navbar.Collapse>
        </Navbar>

    );
}

