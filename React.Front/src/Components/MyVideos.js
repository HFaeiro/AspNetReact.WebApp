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
        if (this.props.profile.videos)
        if (this.props.profile.videos.length > 0)
            this.getUsersVideos();


        //    }
        //    else {
        //        this.props.incrementPollCount()
        //    }
        //}
    }
    deleteVideo = async (e) => {
        fetch(process.env.REACT_APP_API + 'video/' + e.target.value, {
            method: 'Delete',
            headers: {

                'Accept': 'application/json',
                'Authorization': 'Bearer ' + this.props.profile.token,
                'Content-Type': 'application/json'
            }

        })
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
                            resolve(null);


                    })
                    .then(data => {
                        if (data != undefined) {
                            this.setState({ videos: data });
                            resolve(data);

                        }
                    },
                        (error) => {
                            //alert(error);
                            resolve(null);
                        }).
                    catch((error) => {

                        resolve(null);
                    })
            }
        })
    }


    


    render() {
        let uploadVideos = 
            this.props.profile ?
                <div>
                    <UploadVideo
                        profile={this.props.profile}
                        updateProfile={this.props.updateProfile }
                    />
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