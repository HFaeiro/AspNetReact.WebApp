import React, { Component } from 'react';
import { Form, Table } from 'react-bootstrap'
import { Navigate, Link } from 'react-router-dom';
import { useRef } from 'react';
import './MyVideos.css';
import { UploadVideo } from './UploadVideo'
import {Videos } from './video'
export class MyVideos extends Component {
    state =
        {
            videos: [],
            fetchedVideo:
            {
                id: null,
                video: null
            },
            updateVideos: false,
            showPlayer: false,
           

        }
    componentDidMount() {
        if (this.props.profile.videos) {
            if (this.props.profile.videos.length > 0) {
                this.getUsersVideos();
            }

            else if (this.props.profile.vidPollCount >= 2) {
                this.getUsersVideos();
                this.props.resetPollCount()

            }
            else {
                this.props.incrementPollCount()
            }
        }
    }
    deleteVideo = async (e) => {
        fetch(process.env.REACT_APP_API + 'video/' + e, {
            method: 'Delete',
            headers: {

                'Accept': 'application/json',
                'Authorization': 'Bearer ' + this.props.profile.token,
                'Content-Type': 'application/json'
            }

        })
        
            var videoIndex = this.props.profile.videos.indexOf(e);
        if (videoIndex >= 0) {
            var profile = this.props.profile;
            profile.videos.splice(videoIndex, 1);
            this.props.updateProfile(profile);
        }
    }

    playVideo = async (e) => {
        return await new Promise(resolve => {
            fetch(process.env.REACT_APP_API + 'video/play/' + e.id, {
                headers: {
                    'Accept': 'application/json',
                    'Authorization': 'Bearer ' + this.props.profile.token,
                    'Content-Type': 'application/json'
                }

            })
                .then(res => {
                    if (res.status == 200)
                        return res.json()
                    else
                        return;


                })
                .then(data => {
                    this.setState(
                        ({
                            fetchedVideo:
                            {
                                id: e.id,
                                video: data
                            }
                        }));
                    resolve(data);
                },
                    (error) => {
                        //alert(error);
                        resolve(null);
                    }).
                catch((error) => {

                    resolve(null);
                })
        })
    }

   getUsersVideos = async () => {
        return await new Promise(resolve => {
            if (this.props.profile.username) {
                fetch(process.env.REACT_APP_API + 'video/' + this.props.profile.userId, {
                    headers: {

                        'Accept': 'application/json',
                        'Authorization': 'Bearer ' + this.props.profile.token,
                        'Content-Type': 'application/json'
                    }

                })
                    .then(res => {
                        if (res.status == 200)
                            return res.json()
                        else
                            return res;


                    })
                    .then(data => {
                        if (data != undefined) {
                            var profile = this.props.profile;
                            profile.videos = [];
                            if (data.status == undefined) {
                                this.setState({ videos: data });
                                data.forEach(video => {
                                    profile.videos.push(video.id);
                                })
                            }
                            this.props.updateProfile(profile);

                            resolve(data);

                        }
                        else{ 
                            resolve(null);
                        }
                    },
                        (error) => {
                            //alert(error);
                            resolve(null);
                        }).
                    catch((error) => {
                        console.log(error);
                        resolve(null);
                    })
            }
        })
    }


    render() {
        //let contents =
        //    <button
        //        name="Id"
        //        value={v.id}
        //        className="btn btn-danger"
        //        onClick={(e) => this.deleteVideo(e).then(() => window.location.reload())}>
        //        Delete
        //        <span className="glyphicon glyphicon-trash"/>
        //         </button>
                            
                    
                
            
            

        let video = this.state.fetchedVideo.video && this.state.showPlayer ?
            <><video className="VideoPlayer" autoPlay controls muted
                src={"data:video/mp4;base64," + this.state.fetchedVideo.video} >
            </video> : {null}</> : <></>


        let uploadVideos = 
            this.props.profile ?
                <div>
                    <UploadVideo
                        profile={this.props.profile}
                        updateProfile={this.props.updateProfile }
                    />
                </div>
                :
                <></>
        
        return (
            <div>
            <div className="mt-5 justify-content-left">
                
                {uploadVideos}
                    <Videos
                        user={this.props.profile}>
                        <button name="Id" className="btn btn-danger"
                            onClick={(e) => this.deleteVideo(e).then(() => window.location.reload())}>
                        Delete
                        </button>
                    </Videos>
                {video}
                </div>
                </div>
           
        );
    }
}