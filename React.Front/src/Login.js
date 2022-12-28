import React, { Component } from 'react';
import { Modal, Button, Row, Col, Form } from 'react-bootstrap';
import { Navigate,useNavigate} from 'react-router-dom';
import { Users } from './Users';
import {AddUsersModal} from './AddUsersModal';

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
                    if (data.status == 400)
                        resolve(null);
                    else {
                        this.props.login(data);
                        resolve(data);
                    }
                }
          )
       })

    }
    
    loader = async (event) => {
        event.preventDefault();
        const loggedIn = this.props.isLoggedIn;
        if (loggedIn === 'false' || loggedIn === undefined) {
            const res = await this.getPostResponse(event);
            if (res != null) {
                //this.props.login(res);
                //localStorage.setItem('token', res);
                //localStorage.setItem('loggedIn', 'true');
                
                
            }
            else {
                this.props.logout();
                localStorage.setItem('token', '');
                //localStorage.setItem('loggedIn', 'false');
                alert("Local Token Reset: " + res);

            }

        }
        else {
            //localStorage.setItem('token', this.state.token);
            localStorage.setItem('loggedIn', this.state.isLoggedIn);
            <Navigate to={"/users"} state={{token : this.state.token}}/>
           
        }
}



    componentDidMount() {

    }
    componentDidUpdate() {
        
    }
    render() {

        const loggedIn = this.props.isLoggedIn;
        if (loggedIn === 'false' || loggedIn === undefined){
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
               {/* {loggedIn ?(*/}
                       <Navigate to={"/users"} props={
                           {
                               token: this.props.token,
                               loggedIn: this.props.loggedIn
                           }} />
                {/*   )*/}
                {/*       : (*/}
                {/*        localStorage.setItem('token', this.state.token),*/}
                {/*        localStorage.setItem('loggedIn', this.state.loggedIn)*/}
                        
                {/*    )*/}
                {/*}*/}

                
                </div>
                //("Token?!: " + localStorage.getItem('loggedIn')),

        
        )
    }



}