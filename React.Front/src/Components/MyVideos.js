import React, { Component } from 'react';
import { Form, Table } from 'react-bootstrap'
import { Navigate, Link } from 'react-router-dom';
import { useRef } from 'react';
import './MyVideos.css';
import { UploadVideo } from './UploadVideo'
export class MyVideos extends Component {
    state =
        {
            videos: [],
            fetchedVideo:
            {
                id: null,
                video: null
            },

            showPlayer: false

        }
    componentDidMount() {
        var videos = this.getUsersVideos();

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
        let contents =
            <div>{this.state.videos.length ? <>
                <Table striped responsive bordered hover variant="dark"
                >
                    <thead>
                        <tr>
                            <th>File Name</th>
                            <th>File Type</th>
                            <th>File Size</th>
                            <th ></th>
                        </tr>
                    </thead>
                    <tbody>
                        {this.state.videos.map(v =>
                            <tr key={v.id}>
                                <td>{v.fileName}</td>
                                <td>{v.description}</td>
                                <td>{v.contentType}</td>
                                
                                <td className="buttons"><button className="btn btn-primary" name="playButton" onClick={() => {
                                    (this.state.showPlayer && this.state.fetchedVideo.id != v.id
                                        ? this.setState({ showPlayer: this.state.showPlayer })
                                        : this.setState({ showPlayer: !this.state.showPlayer }));
                                    if (this.state.fetchedVideo.id != v.id)
                                        this.playVideo(v);

                                }
                                }>
                                    {this.state.showPlayer && this.state.fetchedVideo.id == v.id ? "Hide" : "Play"}
                                </button><button   name="Id" value={v.id} className="btn btn-danger" onClick={(e) => this.deleteVideo(v.id).then(() => window.location.reload())}>
                                        Delete Video<span className="glyphicon glyphicon-trash"></span>
                                    </button></td>
                            </tr>)}
                    </tbody>
                </Table>
            </> : <>
            </>}
                

            </div>

        let video = this.state.fetchedVideo.video && this.state.showPlayer ?
            <><video className="VideoPlayer" autoPlay controls muted
                src={"data:video/mp4;base64," + this.state.fetchedVideo.video} >
            </video> : {null}</> : <></>


        let uploadVideos = 
            this.props.profile ?
                <div>
                    <UploadVideo
                        profile={this.props.profile}
                    />
                </div>
                :
                <></>
        
        return (
            <div>
               
            
            <div className="mt-5 justify-content-left">
                
                {uploadVideos}
                {contents}
                {video}
               

                </div>
                </div>
           
        );
    }


}