import React, { Component } from 'react';
import { Routes, Route } from 'react-router';
import { VideoApp } from './VideoApp/VideoApp'
import { ReactComponent as BannerLogo } from '../images/AeiroSoftLogo.svg'
import { Home } from './Home';
import Navigation from './Navigation';
import ErrorPage from './ErrorPage';
import './App.css';

export default class App extends Component {

    setStateAsync = async (state) => {
        return await new Promise(resolve => {
        
            this.setState(state, resolve);
        })
    }


    
    render() {
        try{
        return (
            
            <section className="App">
                 <BannerLogo className="bannerLogo"/>
                <Navigation />

                <Routes>
                <Route path="/" element={<Home/>} />
                <Route path="/videoapp/*" element={<VideoApp/>} />
                <Route path="/youtube" element={<></>} />
                <Route path="/github" element={<></>} />
                <Route path="/friends" element={<></>} />
                </Routes>
                
            </section>
            
            
            
            
            
            
            
            
            );
            }
            catch(e)
            {
                console.log(e);
            }

    }
   
}
