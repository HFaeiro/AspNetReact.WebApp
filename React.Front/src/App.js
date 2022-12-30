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
        this.state = { loggedIn: 'false'};
    }


    //lets logout now....
    logout = () => {


        const l = JSON.parse(localStorage.profile);
        if (l.token || l.userId || l.username || l.privileges) { //check profile data if any exists we wipe 
            localStorage.setItem('profile', JSON.stringify({
                userId: '',
                token: '',
                username: '',
                privileges: '',
            }));
            this.setState(
                { loggedIn: 'false' }); //this is where I get warning "cannot update during an existing state transition" wasn't getting this before.. tracing steps back now...
        }

            <Navigate to={"/"} />

    }


    //lets store login data... 
    login = (loginData) => {
        localStorage.setItem('profile', JSON.stringify(loginData));
    }

    
    render() {
        //get our profile data
        var localProfile = localStorage.profile;
        var loggedIn = 'false';
        var profile;
        if (localProfile) { //if the profile exists we will try to use it 

             profile = JSON.parse(localProfile); //lets parse the data now that we know it exists.
            loggedIn = profile.token && profile.userId && profile.username && profile.privileges ? 'true' : 'false'; //lets determine if we're logged in. 
        }
        else { //no data existed.. Usually meant data was null or undefined. we'll create it now. 
            profile = {
                userId: '',
                token: '',
                username: '',
                privileges: '',
            }
            localStorage.setItem('profile', JSON.stringify(profile)); //setup blank tables in the Local item. 
           
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
                    />
                    <Routes>

                        <Route path="/" element={<Home
                            profile={profile }
                        />} />
                        <Route path="/login" element={<Login
                            isLoggedIn={loggedIn}
                            login={this.login}
                            logout={this.logout}
                            token={profile.token}
                        />} />
                        <Route path="/users" element={<Users
                            isLoggedIn={loggedIn}
                            token={profile.token }

                        />} />
                        <Route path="/logout" element={<Logout
                            isLoggedIn={loggedIn }
                            logout={this.logout}


                        />} />


                    </Routes>
                </div>
            </section>
        );
    }


}
