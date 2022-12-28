import React, { Component } from 'react';
import { Home } from './Home';
import { Users } from './Users';
import { Login } from './Login';
import  Navigation  from './Navigation';
import { Routes, Route } from 'react-router';
import { Navigate } from 'react-router-dom'
import ErrorPage from './ErrorPage';
import  Logout  from './Logout';



export default class App extends Component {
    constructor(props) {
        super(props);
        this.login = this.login.bind(this);
        this.logout = this.logout.bind(this);
        this.state = {
            loggedIn: 'false',
            token: ''
        }
    }

    logout = () => {
        console.log(this.state);
        if (this.state.loggedIn === 'true') {
            this.setState({ loggedIn: 'false', token: '' });
            alert("You have been Logged out!");
        }
    }

    login = (token) => {
        this.setState({ loggedIn: 'true', token: token }); //Need to figure out how to get token stored here....
        <Navigate to={"/users"} />
        //alert("You have been Logged In!");
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

                        isLoggedIn={loggedIn}
                        login={this.login }
                        logout={this.logout}

                    />
                    <Routes>

                        <Route path="/" element={<Home />} />
                        <Route path="/login" element={<Login
                            isLoggedIn={loggedIn}
                            login={this.login}
                            logout={this.logout}
                            token={this.state.token}
                        />} />
                        <Route path="/users" element={<Users
                            isLoggedIn={loggedIn}
                            token={this.state.token }

                        />} />
                        <Route path="/logout" element={<Logout
                            logout={this.logout}


                        />} />


                    </Routes>
                </div>
            </section>
        );
    }


}
