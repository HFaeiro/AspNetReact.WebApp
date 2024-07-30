import React, { Component } from 'react';
import { Home } from './Home';
import { UserRouter } from './Users';
import { Login } from './Login';
import  Navigation  from './Navigation';
import { Routes, Route , Navigate} from 'react-router';
import  Logout  from './Logout';
import {AddUsersModal} from './AddUsersModal'
import { MyVideos } from './MyVideos'
import { Videos, VideoRoute } from './Video'
import './VideoApp.css'
export class VideoApp extends Component {
    constructor(props) {
        super(props);
        this.login = this.login.bind(this);
        this.logout = this.logout.bind(this);
        this.updateProfile  = (profile) => this.updateProfile.bind(this);
        this.state = {
            loggedIn: 'false',
            profile: {
                userId: '',
                token: '',
                username: '',
                privileges: '',
                vidPollCount: 0
            }, logoutModal: true, myHistory: null , expiration: null};
        
    }
    setStateAsync = async (state) => {
        return await new Promise(resolve => {
        
            this.setState(state, resolve);
        })
    }
    updateProfile = (profile) =>
    {
        var _profile = this.getProfile();
        _profile.username = profile.username;
        _profile.privileges = profile.privileges;

        localStorage.setItem('profile', JSON.stringify(_profile));

    }
    resetPollCount = () => {

        var profile = this.getProfile();
        profile.vidPollCount = 0;
        this.updateProfile(profile);

    }
    incrementPollCount = () => {
        var profile = this.getProfile();
        profile.vidPollCount++;
        this.updateProfile(profile);
        

    }
    componentDidMount() {
        var profile = this.getProfile();
        var loggedIn = profile.token && profile.userId && profile.username && profile.privileges ? 'true' : 'false';
        this.setState({ loggedIn: loggedIn });

        if (loggedIn == 'true' && profile.token != '') {
            const promise = this.pingServer(profile);
            promise.then(value => {
                if (value == null) {

                    loggedIn = false;
                    this.logout();
                    
                }
                else {
                    loggedIn = true;
                }
           } )
        
    }
        //else if (this.state.fetchedVideo.video == null) {
        //    let vidId = parseInt(this.props.router.params.id);
        //    if (vidId) {
        //        var res = this.getVideo(vidId);
        //    }
        //}

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
    getProfile = () => {
        var localProfile = localStorage.profile;
        
        var profile;
        if (localProfile) { //if the profile exists we will try to use it 

            profile = JSON.parse(localProfile); //lets parse the data now that we know it exists.
            
        }
        else { //no data existed.. Usually meant data was null or undefined. we'll create it now. 
            profile = {
                userId: '',
                token: '',
                username: '',
                privileges: '',
                vidPollCount: 0
            }
            localStorage.setItem('profile', JSON.stringify(profile)); //setup blank tables in the Local item. 

        }
        return profile;
    }

    async pingServer(profile) {
        return await new Promise(resolve => {
            fetch('/' + process.env.REACT_APP_API + 'users/ping' , {
                headers: {
                    'Authorization': 'Bearer ' + profile.token,
                    'Accept': 'application/json',
                    
                }

            })
                .then(res => {
                    if (res.status == 200) {
                        return(res.json());
                    }
                    else if (res.status == 504)
                    {
                        resolve(this.pingServer(profile));
                    }
                    else if(res.status == 401)
                    {
                        resolve(null);
                    }

                }).then(data =>
                {
                    if (data)
                        resolve(data);
                    else {
                        resolve(null);
                    }
                },
                    (error) => {
                        resolve(null);
                    }).
                catch((error) => {
                    resolve(null);
                })
        } )
       

    }
    

    render() {
        //get our profile data
        var profile = this.getProfile();
        //lets determine if we're logged in. 
        var loggedIn = this.state.loggedIn;     
        //setup routes and send props
        return (
            <section className="VideoApp">
                   
                    {profile.username ?
                        <>
                            <h3>Hello, {profile.username}!</h3>

                        </>
                        : <div><h3>Hello stranger!</h3></div>}
                    <Navigation 
                        isLoggedIn={loggedIn}
                        profile={profile }
                    />
                    <Routes>
                        
                        <Route path="/" element={<Home
                            profile={profile}
                            updateProfile={this.updateProfile}
                            incrementPollCount={this.incrementPollCount }
                            resetPollCount={this.resetPollCount }
                        />} />
                        <Route path="login" element={<Login
                            isLoggedIn={loggedIn}
                            login={this.login}
                            logout={this.logout}
                            token={profile.token}
                        />} />
                        <Route path="/users" element={<UserRouter
                            profile={profile }
                            isLoggedIn={loggedIn}
                            updateProfile={this.updateProfile}
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
                        <Route path="/myvideos" element={<MyVideos
                            profile={profile}
                            updateProfile={this.updateProfile}
                            incrementPollCount={this.incrementPollCount}
                            resetPollCount={this.resetPollCount}
                           
                        />} />
                        
                        <Route path="/videos" element={<VideoRoute
                            token={profile.token}
                        />} />
                    <Route path="/play/:id" element={<VideoRoute
                            token={profile.token}
                        />} />

                    </Routes>
                
            </section>
        );
    }
}
