import React, { Component, Profiler, useEffect } from 'react';

import { Navbar, Nav } from 'react-bootstrap';
import { NavLink, Link, useLocation } from 'react-router-dom'

export default function Navigation(props) {





    const login = () => {
        props.login();
    }
    const logout = () => {
        props.logout();
    }



    //useEffect(() => {
    //    console.log(props);
    //}
    //   )


    let NavLog = props.isLoggedIn === 'false' ?
        <NavLink className="d-inline p-2 bg-dark text-white" to="/login"

            state={{
                token: '',
                isLoggedIn: props.isLoggedIn,
                login: props.login
            }}>
            Login
        </NavLink>
        :
        <>
            <NavLink className="d-inline p-2 bg-dark text-white" to="/logout">
                Logout
            </NavLink>
            <NavLink className="d-inline p-2 bg-dark text-white" to="/users">
                Users
            </NavLink>
        </>




    return (
        <Navbar bg="dark" expand="lg">
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

