import React, { Component } from 'react';
import { Table } from 'react-bootstrap'
import VideoTable from './Functions/VideoTable'
   


export class Videos extends Component {
    contructor(props) {
        super(props);
        this.onPlay = this.onPlay.bind(this);
    }
    state =
        {
            videos: [],
            fetchedVideo:
            {
                id: null,
                video: null
            },
            updateVideos: false,
            showPlayer: false

        }
    componentDidMount() {
        if (this.props.user) {
            getVideos();
        }
    }
    onPlay() {
        this.state.showPlayer && this.state.fetchedVideo.id != v.id
            ? this.setState({ showPlayer: this.state.showPlayer })
            : this.setState({ showPlayer: !this.state.showPlayer }));
        if (this.state.fetchedVideo.id != v.id)
            this.playVideo(v);
    }
    getVideo = async (e) => {
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

    getVideos = async () => {
        return await new Promise(resolve => {
            if (this.props.profile.username) {
                fetch(process.env.REACT_APP_API + 'video/' + this.props.user.userId, {
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

        let contents =
            <div>{this.state.videos.length ? <>
                <VideoTable
                    onPlay={this.onPlay}
                    videos={this.videos}
                    showPlayer={this.state.showPlayer}
                />

            </> : <>
            </>}
            </div>

        let video = this.state.fetchedVideo.video && this.state.showPlayer ?
            <><video className="VideoPlayer" autoPlay controls muted
                src={"data:video/mp4;base64," + this.state.fetchedVideo.video} >
            </video> : {null}</> : <></>



        return (
            <div>


                <div className="mt-5 justify-content-left">


                    {contents}
                    {video}


                </div>
            </div>

        );
    }
}