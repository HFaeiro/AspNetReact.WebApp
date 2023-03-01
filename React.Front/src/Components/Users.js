import React, { Component } from 'react';
import { ButtonToolbar, Button } from 'react-bootstrap'
import { AddUsersModal } from './AddUsersModal';
import { EditUsersModal } from './EditUsersModal';
import { DeleteUsersModal } from './DeleteUsersModal';
import { Navigate } from 'react-router-dom';

export class Users extends Component {
    constructor(props) {
        super(props);
        this.state = {
            token: '',
            loggedIn: '',
            profiles: [],
            loading: true
        }
    }

    //refreshes list
    refreshList = async () => {
      //returns promise as the profile[] or null. with token. 
        return await new Promise(resolve => {
            fetch('../' + process.env.REACT_APP_API + 'users', {
                headers:
                {
                    'Accept': 'application/json',
                    'Authorization': 'Bearer ' + this.props.profile.token,
                    'Content-Type': 'application/json' 
                }
            })
                .then(res => res.json())
                .then(data => {
                    this.setState({ profiles: data, loading: false });
                    resolve(data);
                },
                    (error) => {
                        //alert(error);
                        resolve(null);
                    })

        })
    }
    //we need to refresh the users list..
    //returns promise we never handle.
    loader = async () => {
            const res = await this.refreshList();
    }

    //when component mounts and we are confirmed logged in we will begin
    componentDidMount() {
        if (this.props.isLoggedIn == 'true') {
            this.loader();
        }
    }


    //this will map the array of profiles received from server and display them. 
    static renderTable(profile, klass) {
        //determines if displaying user is admin.
        var isAdmin = klass.props.profile.privileges == 'Admin' ?
            true
            : false;

        return (
            <>

                <table className='table table-striped'
                    aria-labelledby="tabelLabel">
                    <thead>
                        <tr>
                            <th>UserId</th>
                            <th>Username</th>
                            <th>Privileges</th>
                        </tr>
                    </thead>
                    <tbody>

                        {profile.map(p =>
                            <tr key={p.userId}>
                                <td>{p.userId}</td>
                                <td>{p.username}</td>
                                <td>{p.privileges}</td>
                                <td>
                                    {/* if admin == true show edit 
                                      and delete users modal buttons */ }
                                    { isAdmin ? <> <EditUsersModal
                                        uId={p.userId}
                                        uName={p.username}
                                        uPass={p.password}
                                        uPriv={p.privileges}
                                        token={klass.props.profile.token}
                                        updateProfile={klass.props.updateProfile }
                                    />
                                    <DeleteUsersModal
                                        uId={p.userId}
                                        uName={p.username}
                                        uPass={p.password}
                                        uPriv={p.privileges}
                                        token={klass.props.profile.token}
                                        />

                                        {p.videos.length ? <>
                                            <button className="btn btn-primary" onClick={() => { }}>
                                                Videos
                                            </button>
                                        </> : <></>}
                                    </>  : <></>}
                                </td>
                            </tr>)}
                    </tbody>
                </table>

                <AddUsersModal
                    token={klass.props.token}


                />



            </>


        );
    }
 

    //if not logged in go to login page.. else render the user tables
    render() {
        let contents = (this.props.isLoggedIn === 'false')
            ?
            <Navigate to={"/login"} props={
                {
                    token: '',
                    isLoggedIn: false
                }} />
            :
            Users.renderTable(this.state.profiles, this);
  
        return (
            <div>
                <h3>Users</h3>

                {contents}
            </div>
        );
    }


}