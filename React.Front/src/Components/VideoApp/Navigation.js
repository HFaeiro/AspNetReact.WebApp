import React, { Component, Profiler, useEffect } from 'react';

import { Navbar, Nav } from 'react-bootstrap';
import { NavLink, Link, useLocation } from 'react-router-dom'
import './Navigation.css';
export default function Navigation(props) {



    //if user is logged in we will show logout instead of login
    let NavLog = props.isLoggedIn === 'false' ? (
        <>
            <Link className="navLink" to="/videoapp/login">
                <div> Login</div>
            </Link>
            <Link className="navLink" to="/videoapp/create">
                <div>Create an Account</div>
            </Link>
        </>
    ) : (

        <>
                <Link className="navLink" to="/videoapp/logout">
                    <div> Logout</div>
                </Link>
                <Link className="navLink" to="/videoapp/myvideos">
                    <div> My Videos</div>
                </Link>
                {/*props.profile.privileges == 'Admin' ?*/ /*// if user is admin we will show Users link.*/
                    <> <Link className="navLink" to="/videoapp/users">
                        <div>Users</div>
                </Link>

        </>
                 
                     }
        </>


        )



    //fill navbar with links and dynamic content. 
    return (
        <Navbar className="Bar" >
            <div className="videoNavBar">
                <Link className="navLink" to="/videoapp/">
                       <div> Home</div>
                    </Link>
                    {NavLog}
                </div>
        </Navbar>

    );
}

