import React, { Component } from 'react';
import { Form, Table } from 'react-bootstrap'
import { Navigate, Link } from 'react-router-dom';
import { useRef } from 'react';
export class Video extends Component {
    state =
        {
            videos: [],
            file: null,
            video: null,
            fetchedVideo:
            {
                id: null,
                video: null
            },
            showResults: false,
            showPlayer: false

        }
    componentDidMount() {
        var videos = this.getUsersVideos();

    }
    clearFiles() {
        window.URL.revokeObjectURL(this.state.video.src);
        this.setState({ file: null, video: null });
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
                        this.setState({ videos: data });
                        resolve(data);
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


    loadVideo = file => new Promise((resolve, reject) => {
        try {
            var video = document.createElement('video');
            video.preload = 'metadata';
            window.URL = window.URL || window.webkitURL;
            video.onloadedmetadata = function () {

                if (video.duration > 90) {

                    reject("Invalid Video! Max Video Length is 1:30s");
                    window.URL.revokeObjectURL(video.src);
                }
                resolve(this);
            }
            video.onerror = function () {

                reject("Invalid File Type - Please upload a video file: " + video.error.message)
                window.URL.revokeObjectURL(video.src);
            }
            video.src = URL.createObjectURL(file)
        }
        catch (e) {

            reject(e);
        }
    })

    getExtention = (filename) => {
        return filename.split('.').pop();
    }
    async loadFile(file) {
        let fileInMB = file.size / 1024 / 1024;
        if (fileInMB <= 0) {
            fileInMB = file.length / 1024 / 1024;

        }
        let ext = this.getExtention(file.name);
        if (ext != "avi") {
            if (ext == 'File') {
                ext = this.getExtention(file.fileName)
            }
            try {
                let video = await this.loadVideo(file);

                if (video && fileInMB < 20) {
                    this.setState({ file: file });
                    this.setState({ video: video });
                }
                else {
                    alert("File Too Powerful, Please upload a file smaller than 20MB");
                    document.getElementById("formFile").value = "";
                    window.URL.revokeObjectURL(video.src);

                }
            }
            catch (e) {
                alert(e);
                document.getElementById("formFile").value = "";
                window.URL.revokeObjectURL(this.state.video.src);
            }
        }
        else {
            alert("Avi No Work, Sorry bud");
            document.getElementById("formFile").value = "";
            window.URL.revokeObjectURL(this.state.video.src);
        }
    }
    async handleFileChange(e) {
        let file = e.target.files[0];
        this.loadFile(file)
    }
    async uploadFile(e) {
        var success = true;
        const formData = new FormData();
        formData.append("Username", this.props.profile.username);
        formData.append("File", this.state.file);
        try {
            await fetch(process.env.REACT_APP_API + 'video/', {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': 'Bearer ' + this.props.profile.token
                },
                body: formData

            }).then(
                response => {// if the response is a JSON object
                    console.log(response);
                },
                (error) => {
                    alert(error);

                })
                .then(
                    console.log("sent file : ", this.state.file.name) // Handle the success response object
                ).catch(
                    error => console.log("fetch: " + error) // Handle the error response object

                );

        }
        catch (e) {
            console.log("catch: " + e)
            success = false;
        }
        if (success) {
            this.setState({ file: null });
            this.setState({ video: null });

        }

    }



    render() {
        let contents = (this.state.file && this.state.video) ?
            <div className=" justify-content-left">

                <button className="btn btn-primary" onClick={(e) => this.setState({ showResults: !this.state.showResults })}>
                    {this.state.showResults ? "Hide" : "Preview"}
                </button>
                <div>
                    {this.state.showResults ? <video width="1080" height="720" controls muted type={this.state.file.type}
                        src={this.state.video.src} >
                    </video> : null}
                </div>
                <table className='table table-striped'
                    aria-labelledby="tabelLabel">
                    <thead>
                        <tr>
                            <th>File Name</th>
                            <th>File Type</th>
                            <th>File Size</th>
                        </tr>
                    </thead>
                    <tbody>

                        <tr key={this.state.file.name}>
                            <td>{this.state.file.name}</td>
                            <td>{this.state.file.type}</td>
                            <td>{this.state.file.size / 1024 / 1024 + " MB"}</td>
                        </tr>
                    </tbody>
                </table>
                <button className="btn btn-primary" onClick={(e) => { this.uploadFile(e).then(() => window.location.reload()) }}>
                    Send File
                </button>
                <button className="btn btn-danger" type="submit" onClick={(e) => this.clearFiles()}>
                    Clear File
                </button>

            </div> :
            <div>{this.state.videos ? <>
                <Table striped responsive bordered hover variant="dark"
                >
                    <thead>
                        <tr>
                            <th>File Name</th>
                            <th>File Type</th>
                            <th>File Size</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        {this.state.videos.map(v =>
                            <tr key={v.id}>
                                <td>{v.fileName}</td>
                                <td>{v.description}</td>
                                <td>{v.contentType}</td>
                                <td><button className="btn btn-primary" name="playButton" onClick={() => {
                                    (this.state.showPlayer && this.state.fetchedVideo.id != v.id
                                        ? this.setState({ showPlayer: this.state.showPlayer })
                                        : this.setState({ showPlayer: !this.state.showPlayer }));
                                    if (this.state.fetchedVideo.id != v.id)
                                        this.playVideo(v);

                                }
                                }>
                                    {this.state.showPlayer && this.state.fetchedVideo.id == v.id ? "Hide" : "Play"}
                                </button><button as="input" name="Id" value={v.id} className="btn btn-danger" onClick={(e) => this.deleteVideo(v.id).then(() => window.location.reload())}>
                                        Delete Video<span className="glyphicon glyphicon-trash"></span>
                                    </button></td>
                            </tr>)}
                    </tbody>
                </Table>
            </> : <>
            </>}
                <div >
                    <Form.Group controlId="formFile" className="mb-3">
                        <Form.Label>Upload a Video Submission!</Form.Label>
                        <Form.Control type="file" name="file_source" size="40" accept="video/*" onChange={(e) => this.handleFileChange(e)} />
                    </Form.Group>

                </div>
            </div>
        let video = this.state.fetchedVideo.id && this.state.showPlayer ?
            <><video width="auto" height="auto" autoPlay controls muted
                src={"data:video/mp4;base64," + this.state.fetchedVideo.video} >
            </video> : {null}</> : <></>






        return (
            <div className="mt-5 justify-content-left">

                {contents}
                {video}

            </div>
        );
    }


}