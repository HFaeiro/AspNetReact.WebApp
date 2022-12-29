import React, { Component } from 'react';
import { Modal, Button, Row, Col, Form } from 'react-bootstrap';
import { Navigate, useNavigate } from 'react-router-dom';
import { Users } from './Users';
import { AddUsersModal } from './AddUsersModal';

export class Login extends Component {
    constructor(props) {
        super(props);


        this.getPostResponse = this.getPostResponse.bind(this);

    }

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
                    //if (data.status == 400)
                    //    resolve(null);
                    //else {
                    //  this.props.login(data);
                    resolve(data);
                    //}
                }
                )
        })

    }

    //send login data to server
    loader = async (event) => {
        const loggedIn = this.props.isLoggedIn;
        //if not logged in yet
        if (loggedIn === 'false' || loggedIn === undefined) {
            event.preventDefault();
            //await login response from server
            const res = await this.getPostResponse(event);
            if (res != null) {
                //if response status is not a bad request send data to login func
                if (res.status != 400) {
                    this.props.login(res);
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
            // localStorage.setItem('loggedIn', this.state.isLoggedIn);
            console.log('should never hit this!?')
            // <Navigate to={"/users"} state={{token : this.state.token}}/>

        }
    }





    componentDidMount() {

    }
    componentDidUpdate() {

    }
    render() {
        //if !loggedin show login form 
        //else redirect to users page
        const loggedIn = this.props.isLoggedIn;
        if (loggedIn === 'false' || loggedIn === undefined) {

            console.log(this.props);
            return (


                <div>
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
                    <AddUsersModal
                        token={this.props.token}


                    />
                </div>

            );
        }
        else
            return (
                <div>
                    <Navigate to={"/"} />

                </div>



            )
    }



}