import React, { Component,useEffect } from 'react';
import { Modal, Button, Row, Col, Form } from 'react-bootstrap';
import { Navigate, useNavigate , useLocation} from 'react-router-dom';

export default function Logout(props) {


    const location = useLocation();
        const { state } = location;





    useEffect(() => {
        console.log(location);
        console.log(props);
    }
       )
    const logout = () => {
        props.logout();
    }
        //localStorage.setItem('token', '');
        //localStorage.setItem('loggedIn', 'false');



        return (
            <div>
                {props.logout()}
               
                <Navigate to={"/login"} />


            </div>
        );
    }




