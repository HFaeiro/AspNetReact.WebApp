import React, { Component } from 'react';
import { Modal, Button, Row, Col, Form } from 'react-bootstrap';
import { Navigate, useNavigate } from 'react-router-dom';
import { Users } from './Users';
import { AddUsersModal } from './AddUsersModal';
import './Login.css'
export class Login extends Component {
    constructor(props) {
        super(props);
        this.getPostResponse = this.getPostResponse.bind(this);

    }
    //attempt to get profile data back from server with login data
    getPostResponse = async (event) => {
        return await new Promise(resolve => {
            fetch(process.env.REACT_APP_API + 'login', {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    Username: event.target.Username.value,
                    Password: event.target.Password.value,
                })
            })
                .then(res => res.json())
                .then(data => {
                    resolve(data); //got the data now lets see what it is! this goes back to loader()
                })
        })

    }

    //send login data to server
    loader = async (event) => {
        const loggedIn = this.props.isLoggedIn;
        if (loggedIn === 'false' || loggedIn === undefined) {//if not logged in yet
            event.preventDefault();
            const res = await this.getPostResponse(event);//await login response from server
            if (res != null) {
                if (res.status != 400) { //if response status is not a bad request send data to login func
                    this.props.login(res);
                    event.target.Username.value = null;
                    event.target.Password.value = null;
                    document.getElementById('login').submit();

                }
                else {
                    alert('Sorry Invalid Username & Password. Please Try Again!');
                    if (this.props.isLoggedIn === 'true')
                        this.props.logout();
                }
            }
            else { //if invalid return value from login we will just send to logout to make sure no lingering logged in vals
                this.props.logout();
            }
        }
        else {//really shouldn't ever hit this....'
            console.log('should never hit this!?')
        }
    }

    render() {
        //if !loggedin show login form 
        const loggedIn = this.props.isLoggedIn;
        if (loggedIn === 'false' || loggedIn === undefined) {

            console.log(this.props);
            return (
                <>
                <div className="loginForm">
                    <h3>Login</h3>

                    <Form id="login" onSubmit={this.loader}>

                        <Form.Group controlId="Username">
                            <Form.Label>Username</Form.Label>
                            <Form.Control type="text" name="Username" required
                            >
                            </Form.Control>
                        </Form.Group>
                        <Form.Group controlId="Password">
                            <Form.Label>Password</Form.Label>
                            <Form.Control type="password" name="Password" required
                            >
                            </Form.Control>
                        </Form.Group>
                        <Form.Group>
                            <Button variant="primary" type="submit">
                                Login
                            </Button>
                        </Form.Group>
                    </Form>
                       
                </div>
                    <div className="">
                        <AddUsersModal
                            token={this.props.token}



                        /> </div>
                </>
            );
        }
        //else redirect to users page
        else
            return (
                <div>
                    <Navigate to={"/"} />

                </div>



            )
    }
}