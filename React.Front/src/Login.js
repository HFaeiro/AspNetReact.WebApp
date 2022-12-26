import React, { Component } from 'react';
import { Modal, Button, Row, Col, Form } from 'react-bootstrap';
import { Navigate,useNavigate } from 'react-router-dom';
import { Users } from './Users';
import {AddUsersModal} from './AddUsersModal';

export class Login extends Component {
    constructor(props) {
        super(props);
        this.state = {
            token: '',
            loggedIn: false
        }
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
                    if (data.status == 400)
                        resolve(null);
                    else {
                        this.setState({ token: data, loggedIn: true });
                        resolve(data);
                    }
                }
          )
       })

    }
    
    loader = async (event) => {
        event.preventDefault();
        if (!this.state.loggedIn) {
            const res = await this.getPostResponse(event);
            const loggedIn = localStorage.getItem('loggedIn');
            if (loggedIn === 'true' && res === null) {
                localStorage.setItem('token', '');
                localStorage.setItem('loggedIn', false);
                alert("Local Token Reset: " + res);
            }
            else {
                localStorage.setItem('token', res);
                localStorage.setItem('loggedIn', 'true');
                <Navigate to={"/users"} state={{ token: this.state.token }} />
            }
        }
        else {
            localStorage.setItem('token', this.state.token);
            localStorage.setItem('loggedIn', this.state.loggedIn);
            <Navigate to={"/users"} state={{token : this.state.token}}/>
            
            //alert("Token?!: " + localStorage.getItem('loggedIn'));

        }
}



    componentDidMount() {

    }
    componentDidUpdate() {

    }
    render() {
        const tokMatch = this.state.token === localStorage.getItem('token');

       if(this.state.loggedIn === false){
        return (


            <div>
            <h3>Login</h3>
                          
                
            <Form onSubmit={this.loader}>

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
            <AddUsersModal />
            </div>

        );
       }
        else
           return (
            <div>
                {tokMatch ?(
                       <Navigate to={"/users"} props={
                           {
                               token: this.state.token,
                               loggedIn: this.state.loggedIn
                           }} />
                   )
                       : (
                        localStorage.setItem('token', this.state.token),
                        localStorage.setItem('loggedIn', this.state.loggedIn)
                        
                    )
                }

                
                </div>
                //("Token?!: " + localStorage.getItem('loggedIn')),

        
        )
    }



}