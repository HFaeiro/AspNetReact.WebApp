import React, { Component } from 'react';
import { Home } from './Home';
import { Users } from './Users';
import { Login } from './Login';
import { Navigation } from './Navigation';
import { Routes, Route } from 'react-router';
import { Navigate } from 'react-router-dom'
import ErrorPage from './ErrorPage';
import { Logout } from './Logout';



export default class App extends Component {
    constructor(props) {
        super(props);
            this.state = {
                loggedIn : false,
                token : ''
            }
    }

    logout = () => {
        this.setState({ loggedIn: false, token: '' });
    }

    render() { 
        const { loggedIn } = this.state;
        return (
            <section className="App">
            <div className="container">
                <h3 className="m-3 d-flex justify-content-center">
                 Login Portal
                </h3>

                    <Navigation
                        ifLoggedIn={loggedIn}
                        logout={ this.logout }

                    />
                    <Routes>
                        
                    <Route path="/" element={<Home />}/>
                    <Route path="/login" element={<Login /> } />
                        <Route path="/users" element={<Users />} />
                        <Route path="/logout" element={<Logout />} />
 
                    
                </Routes>
            </div>
            </section>
        );
    }


}
