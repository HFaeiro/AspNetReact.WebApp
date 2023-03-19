import React from 'react';
import { cloneElement } from 'react';
import './VideoTable.css'
import { ReactComponent as ThumbnailPlaceHolder } from '../../../images/Friends.svg'

function VideoTable(props) {

    let contents =

        <section className="VideoSection">

            
                {props.videos.map(v =>
                    <div className="videoObject" key={v.id}>
                        <div className="videoTitle">{v.title}</div>
                        <a href={'/videoapp/play/' + v.id}><ThumbnailPlaceHolder className="thumbnail"/></a>
                        <div className="buttons">
                            {/*<button className="btn btn-primary" name="playButton" onClick={() =>  props.onPlay(v) */}
                            {/*}>*/}
                            {/*    {props.showPlayer && (props.video ? props.video.id : null) == v.id ? "Hide" : "Play"}*/}
                            {/*</button>*/}
                            {props.children ?
                                props.children.length <= 1 ? cloneElement(props.children, { value: v.id }) :
                                    React.Children.map(props.children, child =>
                                        child.$$typeof && cloneElement(child, { value: v.id , video : v })) : null
                               
                                
                              }
                        </div>
                    </div>
                )}
            
        </section>

    return (
        <div>
            <div className="mt-5 justify-content-left">

                {contents}

            </div>
        </div>

    );
} export default VideoTable