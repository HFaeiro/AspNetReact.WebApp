import React, { Component } from 'react';
import { Home } from './Home';
import { Users } from './Users';
import { Login } from './Login';
import  Navigation  from './Navigation';
import { Routes, Route } from 'react-router';
import { Navigate } from 'react-router-dom'
import ErrorPage from './ErrorPage';
import  Logout  from './Logout';
import {AddUsersModal} from './AddUsersModal'
import { Video } from './videos'

export default class App extends Component {
    constructor(props) {
        super(props);
        this.login = this.login.bind(this);
        this.logout = this.logout.bind(this);
        this.state = { loggedIn: 'false', logoutModal : true };

    }
    setStateAsync = async (state) => {
        return await new Promise(resolve => {
        
            this.setState(state, resolve);
        })
    }

    //lets logout now....
    logout = () => {
        const l = JSON.parse(localStorage.profile);
        if (l.token || l.userId || l.username || l.privileges) { //check profile data if any exists we wipe
            localStorage.clear();//clear local storage.. we don't need anything here atm..
                                //maybe will need to change that in the future
            this.setState(
                { loggedIn: 'false' }); 
        }
    }

    //lets store login data... 
    login = (loginData) => {
        localStorage.setItem('profile', JSON.stringify(loginData));
        this.setState(
            { loggedIn: 'true' }); 
    }

    render() {
        //get our profile data
        var localProfile = localStorage.profile;
        var loggedIn = this.state.loggedIn;
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
                        profile={profile }
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
                            profile={profile }
                            isLoggedIn={loggedIn}
                        />} />
                        <Route path="/logout" element={<Logout
                            isLoggedIn={loggedIn}
                            logout={this.logout}
                            setStateAsync={this.setStateAsync }
                            showModal={this.state.logoutModal}

                        />} />
                        <Route path="/create" element={<AddUsersModal
                            showModal={true}
                            dontShowButton={true }
                        />} />
                        <Route path="/videos" element={<Video
                            profile={profile}
                        />} />

                    </Routes>
                </div>
            </section>
        );
    }
}
