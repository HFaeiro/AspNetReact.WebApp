import React, { Component } from 'react';
import { Form, Modal } from 'react-bootstrap'
import './UploadProgress.css';
import { EditVideosModal } from './EditVideosModal';
export class UploadProgress extends Component {
    constructor(props) {
        super(props);
        this.token = this.props.token;
        this.state =
        {
            showModal: true,
            processingProgress: null,
            taskId: this.props.taskId,
            done: null,
            uploadProgress:
            {
                percentDone: null,
                eta: null,
                speed : null
            },
            chunkCount: this.props.chunkCount,           
            canceled: null,
            confirmedSent: this.props.confirmedSent,
            uploaded: false,
        }
    }
    openModal = () => this.setState({ showModal: true });
    closeModal = () => this.setState({ showModal: false });

    async componentDidMount() {

    }

    async calculateUploadProgress() {
        try {
            if (this.state.chunkCount && this.state.confirmedSent >= 0) {
                let uploadProgress =
                {
                    percentDone: (this.props.confirmedSent / this.state.chunkCount) * 100,
                    eta: "Not Calculated",
                    speed: "not Calulated"
                };
                this.uploadProgress = uploadProgress;

            }
            else {
                this.setState(
                    {
                        uploaded: true
                    }
                );
            }
        } catch (e) {
            console.log("catch: " + e)
            
        }
    }
    async getUploadProcessingStatus(taskId) {
        try {
            await fetch('/' + process.env.REACT_APP_API + 'video/progress/' + taskId, {
                headers: {
                    'Accept': 'application/json',
                    'Authorization': 'Bearer ' + this.token
                }
            }).then(response => {
                if (response.status === 200) {
                    return response.json();
                }
            }, (error) => {
                console.log(error);
            }).then(progress => {
                if (progress) {
                    this.setState(
                        {
                            processingProgress: progress
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
            this.props.success = false;
        }
    }



    render() {
        if ((!this.processingProgress || this.processingProgress.percentDone < 100) && this.props.taskId && (this.uploadProgress && this.uploadProgress.percentDone >= 100) && !this.state.done) {
            this.getUploadProcessingStatus(this.props.taskId);
        }

        if (!this.calcuating && this.lastChunk !== this.props.confirmedSent && this.props.confirmedSent >= 0) {
            this.calcuating = true;
            this.calculateUploadProgress()
                .then(function () {
                    this.lastChunk = this.props.confirmedSent;
                    this.calcuating = false;
                    console.log(this.uploadProgress);
                    if (this.uploadProgress && this.uploadProgress.percentDone >= 100) {
                        console.log("Done Uploading!")
                        if (this.props.taskId) {
                            this.getUploadProcessingStatus(this.props.taskId);
                        }
                    }
                }.bind(this)

                );

        }
        let progressBar =
            <>
                {
                    !this.state.done
                        ?
                        
                        this.state.processingProgress
                                ?
                                <div className="progress">
                                <div className="progressText1"> ETA:{this.state.processingProgress.item1} s </div>
                                < span className="progressText2" > {this.state.processingProgress.item2} % </span>
                                < span className="progressBar" style={{ width: this.state.processingProgress.item2 + '%' }}> </span>
                                </div>
                            :
                            <div>                            
                                {
                                    this.uploadProgress && this.uploadProgress.percentDone
                                    ?
                                    <div className="progress">
                                        <div className="progressText1"> ETA:{this.uploadProgress.eta} s </div>
                                        < span className="progressText2" > {this.uploadProgress.percentDone} % </span>
                                        < span className="progressText3" > {this.uploadProgress.speed} mb/s </span>
                                        < span className="progressBar" style={{ width: this.uploadProgress.percentDone + '%' }}> </span>
                                    </div>
                                    : <></>
                                }
                            </div>                            
                        :
                        <>
                                    {/*    <EditVideosModal*/}
                                    {/*        showModal={false}*/}
                                    {/*        token={this.props.token}*/}
                                    {/*        video={*/}
                                    {/*            this.props.video.isPrivate !== undefined ? this.props.video :*/}
                                    {/*                {*/}
                                    {/*                    isPrivate: "True",*/}
                                    {/*                    title: this.state.file.name,*/}
                                    {/*                }*/}
                                    {/*        }*/}
                                    {/*        taskId={this.props.taskId}*/}
                                    {/*        editParent={this.updateVideoInfo}*/}
                            {/*/>*/}
                                    </>

                }
            </>



       let uploadModal = 
            <Modal className="uploadProgressModal" show={this.state.showModal}
                onHide={this.closeModal}
                size="lg"
                aria-labelledby="contained-modal-title-vcenter"
                centered >
                <Modal.Header >
                    <Modal.Title id="contained-modal-title-vcenter">
                       {
                         this.state.done ?
                               "Video Has been Processed! Continue editing or Finish" : this.uploadProgress && this.uploadProgress.percentDone >= 100
                                   ? "Uploaded! Please wait while we Process it!" : " Uploading! Please don't close this window!"}
                    </Modal.Title>
                </Modal.Header>
                <Modal.Body >
                    <div>                        
                        {progressBar}
                    </div>
                </Modal.Body>
                <Modal.Footer>
                    {this.state.done ?
                        <button className="btn btn-success" onClick={(e) => { this.closeModal(); window.location.reload() }}>
                            Finish
                        </button>
                        : <></>
                    }
                </Modal.Footer>
            </Modal>            

        return (
            <div>
                {uploadModal}
            </div>

        );

    }
}