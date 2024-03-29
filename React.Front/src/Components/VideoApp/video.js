import React, { Component } from 'react';
import { Table } from 'react-bootstrap'
import VideoTable from './Functions/VideoTable'
import { Navigate, useParams } from 'react-router-dom';
import { withRouter } from '../../Utils/withRouter';
import { Helmet } from "react-helmet";
import './Video.css';

export class Videos extends Component {
    constructor(props) {
        super(props);
        this.onPlay = this.onPlay.bind(this);
        this.state =
        {
            videos: [],
            fetchedVideo:
            {
                id: null,
                video: null
            },
            updateVideos: false,
            showPlayer: false,
            userId: props.router ? props.router.location.state ? props.router.location.state.userId : null : this.props.userId,

            token: props.token
        }
        
    }
    
    async componentDidMount() {
        if (this.state.userId) {
            this.getVideos();
        }
        else if (this.state.fetchedVideo.video == null){
            let vidId = parseInt(this.props.router.params.id);
            if (vidId) {
               var res = this.getVideo(vidId);
            }
        }

    }
    onPlay(video) {
        this.state.showPlayer && this.state.fetchedVideo.id != video.id
            ? this.setState({ showPlayer: this.state.showPlayer })
            : this.setState({ showPlayer: !this.state.showPlayer });
        if (this.state.fetchedVideo.id != video.id)
            this.getVideo(video.id);
    }
     loadVideo = file => new Promise((resolve, reject) => {
         try {
             var video = document.createElement('video');
             video.preload = 'metadata';
             window.URL = window.URL || window.webkitURL;
             video.onloadedmetadata = function () {
                 
                 //if (video.duration > 90) {

                 //    reject("Invalid Video! Max Video Length is 1:30s");
                 //}
                 resolve(this);
             }
             video.onerror = function () {
                 reject("Invalid File Type - Please upload a video file")
             }
             video.src = URL.createObjectURL(file)
         }
         catch (e) {
             reject(e);
         }
     })
    getVideo = async (e) => {
        return await new Promise(resolve => {
            fetch('/' + process.env.REACT_APP_API + 'video/play/' + e, {
                headers: {
                    'Authorization': 'Bearer ' + this.state.token,
                    'Accept' : '*/*',
                    'Accept-Encoding': 'gzip, deflate, br',
                    'Connection' : 'keep-alive'
                }

            })
                .then(res => {
                    if (res.status == 200)
                        return res.blob();
                    else
                        return;


                })
                .then(data => {
                    
                    var video = URL.createObjectURL(data);
                    
                    this.setState(
                        ({
                            fetchedVideo:
                            {
                                id: e,
                                video: video
                            }
                        }));
                    resolve(data);
                },
                    (error) => {
                        resolve(null);
                    }).
                catch((error) => {
                    resolve(null);
                })
        })
    }

    getVideos = async () => {
        return await new Promise(resolve => {
            fetch('/' +process.env.REACT_APP_API + 'video/' + this.state.userId , {
                    headers: {

                        'Accept': 'application/json',
                        'Authorization': 'Bearer ' + this.state.token,
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
            
        })
    }
    render() {

        let contents =
            <div>{this.state.videos.length > 0 ? <>
                <VideoTable
                    onPlay={this.onPlay}
                    videos={this.state.videos}
                    showPlayer={this.state.showPlayer}
                    video={this.state.fetchedVideo }
                >{this.props.children }
                </VideoTable>

            </> : <>{this.state.fetchedVideo.video && (this.state.showPlayer || this.props.router && this.props.router.params.id)
                    ?
                    <div>
                        
                        <p><video className="VideoPlayer" controls muted
                        src={this.state.fetchedVideo.video} >
                    </video></p></div>
                    : this.props.userId ? <>No Videos! Please upload one.</>
                        : this.state.userId ? <>This Users Videos are Private!</>
                            : <>This Video Does Not Exist! Please check your Link and Try Again!</>}
            </>}
            </div>





        return (
            <div>
                 {contents}
                
            </div>

        );
    }
} export const VideoRoute = withRouter(Videos)