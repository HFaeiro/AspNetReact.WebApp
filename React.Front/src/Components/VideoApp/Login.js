import React, { Component } from 'react';
import { Modal, Button, Row, Col, Form } from 'react-bootstrap';
import { Navigate, useNavigate } from 'react-router-dom';
import { AddUsersModal } from './AddUsersModal';
import { ValidateCodeModal } from './ValidateCodeModal'
import './Login.css'
export class Login extends Component {
    constructor(props) {
        super(props);
        this.getPostResponse = this.getPostResponse.bind(this);
        this.state =
        {
            showCodeValidation: false,
        }
    }
    //attempt to get profile data back from server with login data
    getPostResponse = async (event) => {

        if (!event.target.Email.value.includes("@")) {
            event.target.Username.value = event.target.Email.value;
            event.target.Email.value = "";
        }
        else {
            event.target.Username.value = "";
        }

        return await new Promise(resolve => {
            fetch('/' + process.env.REACT_APP_API + 'login', {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                body:
                    JSON.stringify({
                        Email: event.target.Email.value,
                        Username: event.target.Username.value,
                        Password: event.target.Password.value,
                    })
                }
               
            )
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
                if (res.status === 200) { //if response status is not a bad request send data to login func
                    let userInfo = await res.json();
                    this.props.login(userInfo);
                    event.target.Email.value = null;
                    event.target.Username.value = null;
                    event.target.Password.value = null;
                    document.getElementById('login').submit();

                }
                else if (res.status === 201) {
                    this.userInfo = await res.json();
                    this.setState(
                        {
                            showCodeValidation: true,
                            uId: this.userInfo.userId,

                        }
                    )
                }
                else {
                    alert('Sorry Invalid Username & Password. Please Try Again!');
                    if (this.props.isLoggedIn === 'true')
                        this.props.logout();
                    event.target.Password.value = null;
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


                    <div>
                        {this.state.showCodeValidation && this.state.uId
                            ? <ValidateCodeModal
                                uId={this.state.uId}
                            /> :
                            <div className="loginForm">


                                <h3>Login</h3>

                                <Form id="login" onSubmit={this.loader}>

                                    <Form.Group controlid="Email">
                                        <Form.Label>Username Or Email</Form.Label>
                                        <Form.Control type="text" name="Email" required>
                                        </Form.Control>
                                    </Form.Group>
                                    <Form.Group controlid="Email">
                                        <Form.Control type="text" name="Username" hidden>
                                        </Form.Control>
                                    </Form.Group>
                                    <Form.Group controlid="Password">
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
                                <div className="">
                                    <AddUsersModal
                                        token={this.props.token}
                                    />
                                </div>
                            </div>} </div>
                </>
            );
        }
        //else redirect to home page
        else
            return (
                <div>
                    <Navigate to={"/videoapp/myvideos"} />

                </div>



            )
    }
}