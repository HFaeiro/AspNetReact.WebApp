import React from 'react';
import { Table } from 'react-bootstrap'
import { cloneElement } from 'react';

function VideoTable(props) {

    let contents =

        <Table striped responsive bordered hover variant="dark">
            <thead>
                <tr>
                    <th>File Name</th>
                    <th>Title</th>
                    <th>Description</th>
                    <th>File Type</th>
                    <th>File Size</th>
                    <th ></th>
                </tr>
            </thead>
            <tbody>
                {props.videos.map(v =>
                    <tr key={v.id}>
                        <td>{v.fileName}</td>
                        <td>{v.title}</td>
                        <td>{v.description}</td>
                        <td>{v.contentType}</td>
                        <td>{(v.contentSize / 1024 / 1024).toFixed(2)}MB</td>
                        <td className="buttons">
                            <button className="btn btn-primary" name="playButton" onClick={() =>  props.onPlay(v) 
                            }>
                                {props.showPlayer && (props.video ? props.video.id : null) == v.id ? "Hide" : "Play"}
                            </button>
                            {props.children ?
                                props.children.length <= 1 ? cloneElement(props.children, { value: v.id }) :
                                    React.Children.map(props.children, child =>
                                        child.$$typeof && cloneElement(child, { value: v.id , video : v })) : null
                               
                                
                              }
                        </td>
                    </tr>
                )}
            </tbody>
        </Table>

    return (
        <div>
            <div className="mt-5 justify-content-left">

                {contents}

            </div>
        </div>

    );
} export default VideoTable