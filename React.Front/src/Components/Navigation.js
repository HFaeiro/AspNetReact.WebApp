import React, { Component, Profiler, useEffect } from 'react';
import { Link,} from 'react-router-dom'
import './Navigation.css';
import { ReactComponent as Logo } from '../images/AeiroSoftLogoInitials.svg'
import { ReactComponent as HomeButton } from '../images/Home.svg'
import { ReactComponent as VideoButton } from '../images/play.svg'
import { ReactComponent as DownloadsButton } from '../images/downloads.svg'
import { ReactComponent as YoutubeButton } from '../images/Youtube.svg'
import { ReactComponent as GithubButton } from '../images/Github.svg'
import { ReactComponent as FriendsButton } from '../images/Friends.svg'
export default function Navigation(props) {



    //if user is logged in we will show logout instead of login



    //fill navbar with links and dynamic content. 
    return (
       <>
          
            <section className="sideWrapper">
                <div className="sidebar">      
                    
                    <Link className="navLink" to="/">
                        <HomeButton className="sidebarButton" />
                        <div> Home</div>
                    </Link>
                    <Link className="navLink" to="/videoapp">

                        <VideoButton className="sidebarButton" />
                        <div>Video App</div>
                    </Link>
                    <Link className="navLink" to="https://github.com/HFaeiro/">

                        <GithubButton className="sidebarButton" />
                        <div>Github</div>
                    </Link>
                    <Link className="navLink" to="http://mikedrones.org">

                        <YoutubeButton className="sidebarButton" />
                        <div>Youtube</div>
                    </Link>
                    <Link className="navLink" to="https://aeirosoft.itch.io/aim-trainer">

                        <DownloadsButton className="sidebarButton" />
                        <div>Downloads</div>
                    </Link>
                    <Link className="navLink" to="/friends">

                        <FriendsButton className="sidebarButton" />
                        <div>My Friends</div>
                    </Link>


                </div>
            
            </section>
        </>
    );
}

