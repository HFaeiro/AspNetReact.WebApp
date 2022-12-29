import React, { Component } from 'react';
import { Home } from './Home';
import { Users } from './Users';
import { Login } from './Login';
import Navigation from './Navigation';
import { Routes, Route } from 'react-router';
import { Navigate } from 'react-router-dom'
import ErrorPage from './ErrorPage';
import Logout from './Logout';



export default class App extends Component {
    constructor(props) {
        super(props);
        this.login = this.login.bind(this);
        this.logout = this.logout.bind(this);

    }




    componentDidMount() {
        const l = JSON.parse(localStorage.profile);

        if (l.token || l.userId || l.username || l.privileges) {
            this.setState(
                {
                    profile: {
                        userId: l.userId,
                        token: l.token,
                        username: l.username,
                        privileges: l.privileges,

                    }
                }
            )
        }
    }


    logout = () => {
        const l = JSON.parse(localStorage.profile);
        if (l.token || l.userId || l.username || l.privileges) {
            localStorage.setItem('profile', JSON.stringify({
                profile: {
                    userId: '',
                    token: '',
                    username: '',
                    privileges: '',

                }
            }));
        }
    }

    login = (loginData) => {
        const profile = JSON.parse(localStorage.profile);

        if (!profile.token) {
            localStorage.setItem('profile', JSON.stringify(loginData));
        }
    }

    render() {
        var profile = JSON.parse(localStorage.profile);
        const loggedIn = profile ? 'true' : 'false';
        if (!profile) {

            profile = {
                userId: '',
                token: '',
                username: '',
                privileges: '',
            }
        }
        //setup routes and send props
        return (
            <section className="App">
                <div className="container">
                    <h3 className="m-3 d-flex justify-content-center">
                        Login Portal
                    </h3>

                    <Navigation

                        isLoggedIn={loggedIn}
                        login={this.login}
                        logout={this.logout}

                    />
                    <Routes>

                        <Route path="/" element={<Home
                            profile={profile}
                        />} />
                        <Route path="/login" element={<Login
                            isLoggedIn={loggedIn}
                            login={this.login}
                            logout={this.logout}
                            token={profile.token}
                        />} />
                        <Route path="/users" element={<Users
                            isLoggedIn={loggedIn}
                            token={profile.token}

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
