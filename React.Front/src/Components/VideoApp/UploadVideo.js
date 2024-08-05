import React, { Component } from 'react';
import { Form, Modal } from 'react-bootstrap'
import { UploadProgress } from './UploadProgress'
import './UploadVideo.css';

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
            filetype : null,
            video: null,
            showResults: false,
            uploaded: false,
            uploadButton: true,
            uploadId: null,
            chunkCount: null,
            currentChunk: null,
            chunkSize: null,
            uploading: false,
            updateBlob: null,
            confirmedSent : null,
        }
        this.sendRetVal =
        {
            success: false,
            taskId: null,
            errorMessage: null,
            uploaded: false,
            uploadButton: true,
            uploadId: null,
            chunkCount: null,
            currentChunk: null,
            chunkSize: null,
            uploading: false,
            updateBlob: null,
            confirmedSent: 0,
        }
    }

    async componentDidMount() {
        if (this.state.uploading && !this.state.uploaded && !this.sending) {
            this.setChunkData();
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
        if (ext && file) {
            if (ext === 'File') {
                ext = this.getExtention(file.fileName)
            }
            try {
                let video = await this.loadVideo(file);

                if (!file.type || file.type.length === 0) {
                    this.fileType = "";
                    switch (ext) {
                        case "mov":
                            this.fileType = "video/quicktime";
                            break;
                        case "avi":
                            this.fileType = "video/x-msvideo"
                            break;
                        default:
                            alert("please upload a different file. sorry.");
                            break;
                    }
                } else {
                    this.fileType = file.type;
                }
                if (file.type.length !== 0 || this.fileType.length !== 0) {

                    if (video && fileInMB <= 2147483648) {
                        this.setState({ file: file });
                        this.setState({ video: video });
                        this.setState({ fileType: this.fileType });
                    }
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
    

    async getChunk(file, chunkIndex, chunkSize) {  

        let begindex = chunkIndex * chunkSize;
        let endex = (chunkIndex + 1) * chunkSize;
        if (endex > file.size) {
            endex = file.size;
        }
        return file.slice(begindex, endex);
    }

    async sendChunk(formData, count = 0) {
        try {
           return await fetch('/' + process.env.REACT_APP_API + 'video/', {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': 'Bearer ' + this.token
                },
                body: formData

            }).then(
                response => {
                    if (response.status === 200) {
                        return response.json()
                    }
                    else if (response.status === 400) {
                        if (count < 5) {
                            this.sendChunk(formData, ++count);
                        }
                        else {
                            return false;
                        }
                    } else if (response.status === 500) {
                        return false;
                    }
                    else if (response.status === 201) {
                        return response.json().then
                            (
                                taskId => {
                                    this.taskId = taskId;
                                    this.setState({
                                        taskId: this.taskId
                                    });
                                    return taskId;
                                }
                            );
                    }
                })
               .then(data => {// if the response is a JSON object 
                   if (data && data !== undefined && data !== "undefined") {
                       console.log("Our Video Upload has returned!", data); // Handle the success response object
                       return data;
                   }
                   else {
                       throw new Error("waduheck");
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
                        return false;
                    }).catch(
                    error => {// Handle the error response object
                            throw new Error("waduheck");
                    }               

                );
        }
        catch (e) {
            console.log("catch: " + e)     
            return false;
        }
    }



    async setChunkData() {
        if (!this.chunkCount && (this.currentChunk === null || this.currentChunk === undefined) && !this.state.chunkSize) {

            this.chunkSize = (1024 * 1024) * 100;
            this.chunkCount = Math.ceil(this.state.file.size / this.chunkSize, this.chunkSize);
            console.log("Calculated Chunks \n\nuploaded filesize is ", this.state.file.size, "with a future chunkCount of", this.chunkCount, "with chunkSize of", this.chunkSize);
            this.uploadBlob = new FormData();
            this.uploadBlob.append("uploadId", null);
            this.uploadBlob.append("videoDuration", this.state.video.duration);
            this.uploadBlob.append("videoHeight", this.state.video.videoHeight)
            this.uploadBlob.append("videoWidth", this.state.video.videoWidth)
            this.uploadBlob.append("chunkCount", this.chunkCount);
            this.uploadBlob.append("contentType", this.state.fileType);
            this.uploadBlob.append("chunkNumber", 0);
            this.currentChunk = 0;
            this.confirmedSent = 0;
            this.sendRetVal.uploading = true;
            this.sendRetVal.uploadButton = false;
            this.setState(
                {
                    chunkSize: this.chunkSize,
                    chunkCount: this.chunkCount,
                    currentChunk: 0,
                    uploading: true,                   
                    uploadButton: false,
                    
                }
            )
        }
        else {
            if (this.currentChunk < this.chunkCount && this.lastChunk !== this.currentChunk && !this.sending ) {
                
                if (this.lastChunk === this.currentChunk - 1 || this.lastChunk === undefined) {
                    this.uploadBlob.set("chunkNumber", this.currentChunk);
                    if (this.uploadId || this.currentChunk === 0) {
                        this.sending = true;
                        await this.uploadFile().then(retVal => {

                            if (retVal.success) {
                                this.lastChunk = this.currentChunk;
                                this.currentChunk++;

                                this.setState(
                                    {
                                        currentChunk: this.currentChunk,
                                        uploadId: retVal.uploadId,
                                        confirmedSent: retVal.confirmedSent,
                                        uploaded: retVal.uploaded,
                                        uploading: retVal.uploading,
                                    }
                                )
                            }
                            else {
                                this.lastChunk = undefined;
                                this.chunkCount = null;
                                this.chunkSize = null;
                                this.currentChunk = null;
                                this.sending = false;
                                this.uploadId = null;
                                this.taskId = null;
                                this.confirmedSent = 0;
                                this.setState(
                                    {
                                        confirmedSent: 0,
                                        uploadButton: true,
                                        chunkCount: null,
                                        chunkSize: null,
                                        currentChunk: null,
                                        uploading: false,
                                        uploaded: false,
                                        errorMessage: "Failed to Upload, Please Try Again!",                                        
                                        success: false
                                    }
                                );
                            }
                        }
                        );
                    }                   
                }
                else if (this.lastChunk > this.currentChunk)
                {
                    throw new Error("Ooops!")
                }
                else {
                    this.currentChunk = this.lastChunk + 1;
                    
                }
            }
        }
    }

   async uploadFile() {
        return await this.getChunk(this.state.file, this.currentChunk, this.chunkSize)
            .then(chunk => {
                this.uploadBlob.set("file", chunk, this.state.file.name);
                this.uploadBlob.set("uploadId", this.uploadId);
                console.log("Sending Chunk Number ", this.currentChunk, " of size:", chunk.size, " for uploadId:", this.state.uploadId);
            }).then(function (){ 
               return this.sendChunk(this.uploadBlob)
                    .then(result => {
                        if (!result) {                            
                            throw new Error("Failed to Upload, Please Try Again!");
                        }
                        if (this.currentChunk < this.chunkCount) {
                            this.uploadId = result;
                            this.confirmedSent++;
                            this.sendRetVal.uploadId = result;
                            this.sendRetVal.confirmedSent = this.confirmedSent;
                            this.sendRetVal.success = true;
                                
                            this.uploadBlob.set("uploadId", result);
                        } else if (this.confirmedSent >= this.chunkCount){
                            this.sendRetVal.uploaded = true;
                            this.sendRetVal.uploading = false;
                            this.sendRetVal.confirmedSent = this.confirmedSent;
                            this.sendRetVal.success = true;
                            
                        }
                        this.sending = false;
                        return this.sendRetVal;
                    }).catch(
                        error => {// Handle the error response object
                            console.log(error);                            
                            this.sendRetVal.success = false;
                            this.sending = false;
                            return this.sendRetVal;
                        }
                );
            }.bind(this));

    }


    render() {
        if (this.state.uploading && !this.state.uploaded && !this.sending) {
            this.setChunkData();
        }
        else if (this.taskId) {
            console.log("Task Id! %d", this.taskId);
        }

        let uploadStatus = 
            this.state.uploading && this.state.confirmedSent ?
                <UploadProgress
                    taskId={this.state.taskId}
                    token={this.token}
                    chunkCount={this.state.chunkCount}
                    // currentChunk={this.currentChunk}
                    confirmedSent={this.state.confirmedSent}

                /> : <></>


                //
        

        let uploadModal = this.state.showModal && !this.state.uploading ?           

                <Modal className="uploadModal" show={this.state.showModal}
                    onHide={this.closeModal}
                    size="lg"
                    aria-labelledby="contained-modal-title-vcenter"
                    centered >
                    <Modal.Header >
                        <Modal.Title id="contained-modal-title-vcenter">
                            {"Upload Video"}
                        </Modal.Title>
                    </Modal.Header>
                <Modal.Body >
                         <div>
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
                                        <button className="btn btn-primary" disabled={!this.state.uploadButton} onClick={(e) => { this.setChunkData(); }}>
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
                            </div>}
                    </div>
                    </Modal.Body>
                    <Modal.Footer>                        
                            <button className="btn btn-danger" disabled={!this.state.uploadButton} onClick={this.closeModal}>
                                Cancel
                            </button>

                        
                    </Modal.Footer>
                </Modal>            

            :            
                <button className="btn btn-primary" onClick={(e) => this.setState({ showModal: !this.state.showModal })}>
                    Upload!</button>
            


        return (
            <div>
                {uploadModal}
                {uploadStatus}
            </div>

        );
    }
}