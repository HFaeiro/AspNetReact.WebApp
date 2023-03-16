import React, { Component } from 'react';
import { Form, Table } from 'react-bootstrap'
import { Navigate, Link } from 'react-router-dom';
import { useRef } from 'react';
import './MyVideos.css';
import { UploadVideo } from './UploadVideo'
import { Videos } from './video'

import { EditVideosModal } from './EditVideosModal'

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
        //if (this.props.profile.videos) {
        //    if (this.props.profile.videos.length > 0) {
        //        this.getUsersVideos();
        //    }

        //    else if (this.props.profile.vidPollCount >= 2) {
        //        this.getUsersVideos();
        //        this.props.resetPollCount()

        //    }
        //    else {
        //        this.props.incrementPollCount()
        //    }
        //}
    }
    editVideo = async (e) => {

    }
    deleteVideo = async (e) => {
        fetch('/' +process.env.REACT_APP_API + 'video/' + e.target.value, {
            method: 'Delete',
            headers: {

                'Accept': 'application/json',
                'Authorization': 'Bearer ' + this.props.profile.token,
                'Content-Type': 'application/json'
            }

        })
       
        var videoIndex = this.props.profile.videos.indexOf(parseInt(e.target.value));
        if (videoIndex >= 0) {
            var profile = this.props.profile;
            profile.videos.splice(videoIndex, 1);
            this.props.updateProfile(profile);
        }
    }

    render() {
        let uploadVideos = 
            this.props.profile ?
                <div>
                    <UploadVideo
                        profile={this.props.profile}
                        updateProfile={this.props.updateProfile }
                    />
                    
                       
                            <Videos
                                userId={this.props.profile.userId}
                                token={this.props.profile.token}>
                                <EditVideosModal
                                    token={this.props.profile.token}
                                />
                                <button name="Id" className="btn btn-danger"
                                    onClick={(e) => this.deleteVideo(e).then(() => window.location.reload())}>
                                    Delete
                                </button>
                            </Videos>

                    
                </div>
                :
                <>An Error Has Occured In Upload Component</>
        
        return (
            <div>
            <div className="mt-5 justify-content-left">
                
                {uploadVideos}
                    
                
                </div>
                </div>
           
        );
    }
}