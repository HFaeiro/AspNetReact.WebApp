import React, {Component, Profiler } from 'react';

import { Navbar, Nav} from 'react-bootstrap';
import {NavLink } from 'react-router-dom'

export class Navigation extends Component {
    constructor(props) {
        super(props);
        this.state = {
            isLoggedIn: localStorage.getItem('loggedIn'),
        }
    }

    render() {
        console.log("Logged In Navigation: " + this.state.isLoggedIn);

        return (
 
                <Navbar bg="dark" expand="lg">
                <Navbar.Toggle aria-controls="basic-navbar-nav"></Navbar.Toggle>
                <Navbar.Collapse id="basic-navbar-nav">
                    <Nav className="ml-auto">
                         <NavLink className="d-inline p-2 bg-dark text-white" to="/">
                            Home
                        </NavLink>
                        {this.state.isLoggedIn == 'true' && ( <NavLink className="d-inline p-2 bg-dark text-white" to="/Logout">
                           Logout
                        </NavLink>)}
                        {this.state.isLoggedIn == 'false'  && (<NavLink className="d-inline p-2 bg-dark text-white" to="/login">
                            Login
                        </NavLink>)}
                        <NavLink className="d-inline p-2 bg-dark text-white" to="/users">
                            Users
                        </NavLink>
                       
                    </Nav>
                </Navbar.Collapse>


            </Navbar>
  
        );
    }


}