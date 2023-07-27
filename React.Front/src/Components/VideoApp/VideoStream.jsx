import React, { Component } from 'react';
import { Table } from 'react-bootstrap'
import { Navigate, useParams } from 'react-router-dom';
import { withRouter } from '../../Utils/withRouter';
import { Helmet } from "react-helmet";
import './Video.css';

export class VideoStream extends Component {
    constructor(props) {
        super(props);
        this.state =
        {
            master:
            {
                GUID: null,
                index:
                    [
                {
                    bandwidth: null,
                    resolution: null,

                 }],

            },
            fetchedVideo:
            {
                id: null,
                video: null
            },
            token: props.token
        }
    }
    parseMaster = async (Master) => {
        if (Master) {
            var master =
            {
                GUID: null,
                indecies: [],
            };
            for (var i in Master) {
                {
                    var line = this.props.master[i];
                    if (/GUID/.test(line)) {
                        var keyVal = line.split('=')[1];
                        master.GUID = keyVal;
                        continue;
                    }
                    if (/STREAM-INF/.test(line)) {
                        var keyVal = line.split(':')[1];
                        if (/BANDWIDTH/.test(keyVal)) {
                            var bandwidth = keyVal.split('=')[1].split(',')[0],
                                resolution = keyVal.split(',')[1].split(',')[0].split('=')[1];

                            master.indecies.push({ bandwidth, resolution });

                            console.log("Resolution Found : " + resolution);
                        }
                    }
                }
            }
            this.setState({
                master:master
            })
        }
    }

    async componentDidMount() {
        if (this.state.fetchedVideo?.video == null) {
            this.parseMaster(this.props.master)
        }

    }

    getVideo = async (GUID) => {
        return await new Promise(resolve => {
            fetch('/' + process.env.REACT_APP_API + 'stream/' + GUID, {
                headers: {
                    'Authorization': 'Bearer ' + this.state.token,
                    'Accept': '*/*',
                    'Accept-Encoding': 'gzip, deflate, br',
                    'Connection': 'keep-alive'
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
                                id: GUID,
                                video: video
                            }
                        }));
                    resolve(data);
                },
                    (error) => {
                        console.log(error);
                        resolve(null);
                    }).
                catch((error) => {
                    console.log(error);
                    resolve(null);
                })
        })
    }




    render() {
        return(<></>);
    }

    
}