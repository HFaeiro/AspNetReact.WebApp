import React, { Component } from 'react';
import { Form, Modal } from 'react-bootstrap'
import './UploadVideo.css';
import { EditVideosModal } from './EditVideosModal';
export class UploadVideo extends Component {
    constructor(props) {
        super(props);
        this.token = this.props.profile.token;
        this.updateVideoInfo = this.updateVideoInfo.bind(this);
        this.state =
        {
            taskId: null,
            errorMessage: null,
            showModal: false,
            file: null,
            video: null,
            showResults: false,
            uploaded: false,
            uploadButton: true,
            progress: null,
            done:false,
        }
    }

    openModal = () => this.setState({ showModal: true });
    closeModal = () => this.setState({ showModal: false });

    clearFile() {
        window.URL.revokeObjectURL(this.state.video.src);
        this.setState({ file: null, video: null });
    }
    updateVideoInfo = (video) => {

        this.setState
            (
                {
                    video:
                    {
                        Id: this.state.taskId,
                        title: video.Title.value,
                        description: video.Description.value,
                        isPrivate: video.Private.value

                    }
                }
            );
    }
    loadVideo = file => new Promise((resolve, reject) => {
        try {
            var video = document.createElement('video');
            video.preload = 'metadata';
            window.URL = window.URL || window.webkitURL;
            video.onloadedmetadata = function () {

                if (video.duration > 900) {

                    reject("Invalid Video! Max Video Length is 15m");
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

                if (video && fileInMB <= 21474836480) {
                this.setState({ file: file });
                this.setState({ video: video });
                }
                else {
                alert("File Too Powerful!, Please upload a file smaller than 2GB");
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

    async getUploadProcessingStatus(taskId) {
        try {
            await fetch('/' + process.env.REACT_APP_API + 'video/progress/' + taskId, {
                headers: {
                    'Accept': 'application/json',
                    'Authorization': 'Bearer ' + this.token
                }
            }).then(response => {
                if (response.status == 200) {
                    return response.json();
                }
            }, (error) => {
                console.log(error);
            }).then(progress => {
                if (progress) {
                    this.setState(
                        {
                            progress: progress
                        }
                    );
                    if (progress.item2 < 100) {
                        setTimeout(1000)
                        this.getUploadProcessingStatus(taskId);
                    }
                    else {
                        this.setState(
                            {
                                done: true
                            }
                        );
                    }
                }

            });

        } catch (e) {
            console.log("catch: " + e)
            this.state.success = false;
        }
    }

    async uploadFile() {
        this.setState(
            {
                uploadButton: false
            }
        )
        var success = true;
        const formData = new FormData();
        formData.append("VideoLength", this.state.video.duration);
        formData.append("File", this.state.file);
        try {
            await fetch('/' + process.env.REACT_APP_API + 'video/', {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': 'Bearer ' + this.token
                },
                body: formData

            }).then(
                response => {
                    if (response.status == 200) {
                        return response.json()

                    }
                    else if
                        (response.status == 400) {
                        throw ("Failed to Upload, Please Try Again!")
                    }
                })
                .then(data => {// if the response is a JSON object
                    if (data) {//we got the task ID back so we can then reference it back for progress updates! 
                        this.setState({
                            taskId: data,
                            uploaded: true

                        });
                        this.getUploadProcessingStatus(data);
                    }

                },
                    (error) => {
                        window.URL.revokeObjectURL(this.state.video.src);
                        this.setState(
                            {
                                errorMessage: error,
                                file: null,
                                video: null
                            });
                        success = false;
                    })
                .then(() => {
                    console.log("sent file : ", this.state.file.name); // Handle the success response object

                }).catch(
                    error => console.log("fetch: " + error), // Handle the error response object

                    /*success = false*/
                );

        }
        catch (e) {
            console.log("catch: " + e)
            success = false;
        }
        /*        
        if (success) {
                    this.setState({ file: null });
                    this.setState({ video: null });
        
        
                }
        */
    }


    render() {
        let uploadModal = this.state.showModal ?
            <div>

                <Modal className="uploadModal" show={this.state.showModal}
                    onHide={this.closeModal}
                    size="lg"
                    aria-labelledby="contained-modal-title-vcenter"
                    centered >
                    <Modal.Header >
                        <Modal.Title id="contained-modal-title-vcenter">
                            {this.state.uploaded ? this.state.done ? "Video Has been Processed! Continue editing or Finish" : "Uploaded! Now Edit Your video while we Process it!" : "Upload Video"}
                        </Modal.Title>
                    </Modal.Header>
                    <Modal.Body >
                        {this.state.uploaded
                            ? <div>
                                <EditVideosModal
                                    showModal={true}
                                    token={this.token}
                                    video={
                                        this.state.video.isPrivate != undefined ? this.state.video :
                                            {
                                                isPrivate: "True",
                                                title: this.state.file.name,
                                            }
                                    }
                                    taskId={this.state.taskId}
                                    editParent={this.updateVideoInfo}
                                />

                                {this.state.progress ?
                                    <div className="progress">
                                        <div className="progressText1">ETA:{this.state.progress.item1}s</div>

                                        <span className="progressText2" >{this.state.progress.item2}%</span>
                                        <span className="progressBar" style={{ width: this.state.progress.item2 + '%' }}> </span>


                                    </div>
                                    : <div></div>}
                            </div>
                            : <div>
                                {(this.state.file && this.state.video && this.props.profile !== undefined)

                                    ?
                                    <div className=" justify-content-left">
                                        <button className="btn btn-primary" onClick={(e) => this.setState({ showResults: !this.state.showResults })}>
                                            {this.state.showResults ? "Hide Preview" : "Preview"}
                                        </button>
                                        <div>
                                            {this.state.showResults
                                                ? <video className="videoPreview" controls muted type={this.state.file.type}
                                                    src={this.state.video.src} >
                                                </video>
                                                : null}
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

                                        <div >
                                            <button className="btn btn-primary" disabled={!this.state.uploadButton} onClick={(e) => { this.uploadFile(); }}>
                                                Upload
                                            </button>
                                            <button className="btn btn-danger" disabled={!this.state.uploadButton} onClick={(e) => { this.clearFile() }}>
                                                Clear
                                            </button>
                                        </div>
                                    </div>
                                    :
                                    <div >
                                        <Form.Group controlid="formFile" className="mb-3">
                                            <Form.Label>{this.state.errorMessage ? this.state.errorMessage : "Upload a Video!"}</Form.Label>
                                            <Form.Control type="file" name="file_source" size="40" accept="video/*" onChange={(e) => this.handleFileChange(e)} />
                                        </Form.Group>

                                    </div>} </div>}
                    </Modal.Body>
                    <Modal.Footer>
                        {this.state.uploaded ?
                            <button className="btn btn-success" onClick={(e) => { this.closeModal(); window.location.reload() }}>
                                Finish
                            </button>
                            :
                            <button className="btn btn-danger" disabled={!this.state.uploadButton} onClick={this.closeModal}>
                                Cancel
                            </button>

                        }
                    </Modal.Footer>
                </Modal>
            </div>

            :
            <div>
                <button className="btn btn-primary" onClick={(e) => this.setState({ showModal: !this.state.showModal })}>
                    Upload!</button>
            </div>


        return (
            <div>
                {uploadModal}
            </div>

        );
    }
}