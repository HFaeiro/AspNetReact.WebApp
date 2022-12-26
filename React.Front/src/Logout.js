import React, { Component } from 'react';
import { Modal, Button, Row, Col, Form } from 'react-bootstrap';
import { Navigate, useNavigate } from 'react-router-dom';

export class Logout extends Component {



    render() {


        localStorage.setItem('token', '');
        localStorage.setItem('loggedIn', 'false');
        alert("You have been Logged out!");


        return (
            
            <Navigate to={"/login"} state={{ token: '', loggedin: false }} />
                );



    }




}