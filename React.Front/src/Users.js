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
            token: localStorage.getItem('token'),
            loggedIn: localStorage.getItem('loggedIn'),
            profile: [],
            loading: true

        }


    }

    refreshList = async () => {
      
        return await new Promise(resolve => {
            fetch('../' + process.env.REACT_APP_API + 'users', {
                headers:
                {
                    'Accept': 'application/json',
                    'Authorization': 'Bearer ' + this.state.token,
                    'Content-Type': 'application/json'
                    
                }
            })
                .then(res => res.json())
                .then(data => {
                    this.setState({ profile: data, loading: false });
                    resolve(data);
                },
                    (error) => {
                        
                        //alert(error);
                        resolve(null);
                    })

        })
        

    }

    loader = async () => {
            const res = await this.refreshList();
    }


    componentDidMount() {
        if (this.state.loggedIn == 'true') {
            this.loader();
        }
    }



    static renderTable(profile, klass) {


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
                                    <EditUsersModal
                                        uId={p.userId}
                                        uName={p.username}
                                        uPass={p.password}
                                        uPriv={p.privileges}
                                        token={klass.state.token}
                                    />
                                    <DeleteUsersModal
                                        uId={p.userId}
                                        uName={p.username}
                                        uPass={p.password}
                                        uPriv={p.privileges}
                                        token={klass.state.token}
                                    />
                                </td>
                            </tr>)}
                    </tbody>
                </table>

                <AddUsersModal />



            </>


        );
    }
 


    render() {
        let contents = this.state.loggedIn
            ? 
             Users.renderTable(this.state.profile, this)
            :
            <Navigate to={"/login"} props={
                {
                    token: '',
                    loggedIn: false
                }} />

        return (
            <div>
                <h3>Users</h3>

                {contents}
            </div>
        );
    }


}