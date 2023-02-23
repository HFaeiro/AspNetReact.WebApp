import React, { Component } from 'react';
import { Form, Table } from 'react-bootstrap'
import { Navigate, Link } from 'react-router-dom';
import { useRef } from 'react';
export class Home extends Component {
    state =
        {
            defaultUser: [],
            videos: [],
            file: null,
            video: null,
            showResults: false
 
        }
    clearFiles() {
        window.URL.revokeObjectURL(this.state.video.src);
        this.setState({ file: null, video: null });
    }
    async getDummyUserInfo() {
        fetch(process.env.REACT_APP_API + 'users/' + '1', {
            method: 'Get',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }

        }).then(res => res.json())
            .then((result) => {
                this.setState({ defaultUser: result });


            },
                (error) => {
                    console.log('Failed:' + error);
                })
    }
    getUsersVideos = async () => {
        return await new Promise(resolve => {
            if (this.props.profile.username) {
                fetch(process.env.REACT_APP_API + 'video/' + this.props.profile.userId, {
                    method: 'Get',
                    headers: {

                        'Accept': 'application/json',
                        'Authorization': 'Bearer ' + this.props.profile.token,
                        'Content-Type': 'application/json'
                    }

                })
                    .then(res => res.json())
                    .then(data => {
                        this.setState({ videos: data});
                        resolve(data);
                    },
                        (error) => {
                            //alert(error);
                            resolve(null);
                        })
            }
        })
    }
            
    async handleSubmit() {
        await this.getDummyUserInfo();
        var videos = await this.getUsersVideos();
        if (videos)
            console.log("video array length = " + videos.length);
        else
            console.log("videos are " + videos);
    }

    componentDidMount() {
        this.handleSubmit();
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
    getExtention = (filename) =>{
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

                if (video && fileInMB < 100) {
                    this.setState({ file: file });
                    this.setState({ video: video });
                }
                else {
                    alert("File Too Powerful, Please upload a file smaller than 1GB");
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
    uploadFile(e) {
        var success = true;
        const formData = new FormData();
        formData.append("Username", this.props.profile.username);
        formData.append("File", this.state.file);
        try {
            fetch(process.env.REACT_APP_API + 'video/', {
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
        if (success)
            this.setState({ file: null });

        
    }
    

    render() {            
        
        let loggedInContents = (this.state.file && this.state.video ) ?
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
                <button className="btn btn-primary" onClick={(e) => this.uploadFile(e)}>
                    Send File
                </button>
                <button className="btn btn-danger" onClick={(e) => this.clearFiles()}>
                    Clear File
                </button>
                
            </div> :
            <div>{this.state.videos.length ? <>
                <Table striped bordered hover variant="dark"
                    >
                    <thead>
                        <tr>
                            <th>File Name</th>
                            <th>File Type</th>
                            <th>File Size</th>
                            <th></th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>

                        {this.state.videos.map(v =>

                            <tr key={v.id}>

                                <td>{v.fileName}</td>
                                <td>{v.description}</td>
                                <td>{v.contentType}</td>
                                <td><button className="btn btn-primary" onClick={(e) => this.clearFiles()}>
                                    Play Video
                                </button></td>
                                <td><button className="btn btn-danger" onClick={(e) => this.clearFiles()}>
                                    Delete Video
                                </button></td>
                            </tr>)}
                    </tbody>
                </Table>
            </> : <>
            </>}
                <div >
                    <Form.Group controlId="formFile" className="mb-3">
                        <Form.Label>Upload a Video Submission!</Form.Label>
                        <Form.Control  type="file" name="file_source" size="40" accept="video/*" onChange={(e) => this.handleFileChange(e)} />
                    </Form.Group>

                </div>





            </div>

        //let contents = username if it exists.
        let contents = this.props.profile.username ?
            < >
                <h3>Hello, {this.props.profile.username}</h3>
                   { loggedInContents }</>
            : <div><h3>Hello stranger!</h3>
                <div><span>
                    You can either create a user or you can login using the default!
                </span>
                </div>
                <span>
                    Try this Username : {this.state.defaultUser.username}
                </span>
                <div> <span>
                    And this Password! : {this.state.defaultUser.password}
                </span>
                </div>
                <div> <span>
                    Might see the password change right here if you edit it
                </span>
                </div>
                <Link to="login" className="btn btn-primary" >
                    Login!
                </Link>
            </div>


       


        return (
            <div className="mt-5 justify-content-left">

                {contents}
                
                
            </div>
        );
    }


}